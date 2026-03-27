using DFe.Classes.Entidades;
using DFe.Classes.Flags;
using Microsoft.AspNetCore.Hosting;
using Model;
using Model.DTO;
using Model.DTO.NFe;
using Model.Enums;
using Model.Moves;
using Model.Registrations;
using Newtonsoft.Json;
using NFe.App;
using NFe.Classes;
using NFe.Classes.Informacoes;
using NFe.Classes.Informacoes.Cobranca;
using NFe.Classes.Informacoes.Destinatario;
using NFe.Classes.Informacoes.Detalhe;
using NFe.Classes.Informacoes.Detalhe.Tributacao;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Federal;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Federal.Tipos;
using NFe.Classes.Informacoes.Emitente;
using NFe.Classes.Informacoes.Identificacao;
using NFe.Classes.Informacoes.Identificacao.Tipos;
using NFe.Classes.Informacoes.Observacoes;
using NFe.Classes.Informacoes.Pagamento;
using NFe.Classes.Informacoes.Total;
using NFe.Classes.Informacoes.Transporte;
using NFe.Classes.Protocolo;
using NFe.Classes.Servicos.Tipos;
using NFe.Danfe.Nativo.NFCe;
using NFe.Servicos;
using NFe.Servicos.Retorno;
using NFe.Utils;
using NFe.Utils.Email;
using NFe.Utils.Evento;
using NFe.Utils.InformacoesSuplementares;
using NFe.Utils.NFe;
using Org.BouncyCastle.Tls;
using Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Service
{
	public class NFeService : BaseService<NFeEmission>, INFeService
	{
		private readonly ISaleRepository _saleRepository;
		private readonly IFiscalConfigurationRepository _fiscalConfigurationRepository;
		private readonly INaturezaOperacaoRepository _naturezaOperacaoRepository;
		private readonly IWebHostEnvironment _environment;
		private NFe.Classes.NFe _nfe;
		private   ConfiguracaoApp _configuracaoApp;
		private FiscalConfiguration _currentFiscalConfiguration;
		private NaturezaOperacao _currentNaturezaOperacao;
		private Sale _currentSale;

		public NFeService(IGenericRepository<NFeEmission> repository,
				ISaleRepository saleRepository,
				IFiscalConfigurationRepository fiscalConfigurationRepository,
				INaturezaOperacaoRepository naturezaOperacaoRepository,
				IWebHostEnvironment webHostEnvironment) : base(repository)
		{
			_saleRepository = saleRepository;
			_fiscalConfigurationRepository = fiscalConfigurationRepository;
			_naturezaOperacaoRepository = naturezaOperacaoRepository;
			_environment = webHostEnvironment;
		}
		public async Task<ResponseGeneric> CreatedFromSale(NFeEmissionDto nfeDto)
		{
			List<NFeEmission> nFeEmissionList = await (repository as INFeRepository).GetBySaleIdAsync(nfeDto.SaleId);

			if (nFeEmissionList.Count > 0)
			{
				NFeEmission nFeEmission = nFeEmissionList.FirstOrDefault(x => x.StatusNfe == StatusNfe.pendente);

				if (nFeEmission != null)
				{
					ResponseGeneric responseGeneric = await Resend(nFeEmission.Id);
					NFeEmission nFeEmissionResp = responseGeneric.Data as NFeEmission;
					if (nFeEmissionResp == null)
					{
						return responseGeneric;

					}
					if (nFeEmissionResp.StatusNfe != StatusNfe.emitida)
					{
						return new ResponseGeneric { Success = false, Message = nFeEmissionResp.ErrorMessage };
					}
					else
					{
						return new ResponseGeneric
						{
							//poder ser a danfe
							Success = true
						};
					}
				}
				return new ResponseGeneric { Success = false, Message = "Não foi encontrado a nota!" };
			}
			else
			{
				ResponseGeneric responseGeneric = await CreateAttemptAsync(nfeDto);
				NFeEmission nFeEmissionResp = responseGeneric.Data as NFeEmission;
				if (nFeEmissionResp == null)
				{
					return responseGeneric;

				}
				if (nFeEmissionResp.StatusNfe != StatusNfe.emitida)
				{
					return new ResponseGeneric { Success = false, Message = nFeEmissionResp.ErrorMessage };
				}
				else
				{
					return new ResponseGeneric
					{
						Success = true
					};
				}
			}
		}

		// Método principal para reenvio
		public async Task<ResponseGeneric> Resend(int id)
		{
			NFeEmission nFeEmission = await (repository as INFeRepository).GetByIdAsync(id);
			if (nFeEmission == null)
				return new ResponseGeneric { Success = false, Message = "Não foi encontrado a nota!" };

			var (validationResult, fiscalConfig, sale, naturezaOperacao) =
					await ValidateAndGetDependencies(nFeEmission.CompanyId, nFeEmission.SaleId, nFeEmission.NaturezaOperacaoId);

			if (!validationResult.Success)
				return validationResult;

			int numeroNfe = Convert.ToInt32(nFeEmission.Numero);
			var respEmissao = await TransmitirNfe(numeroNfe, fiscalConfig, sale, naturezaOperacao);

			UpdateNFeEmission(nFeEmission, respEmissao, fiscalConfig);

			await repository.UpdateAsync(nFeEmission.Id, nFeEmission);
			return new ResponseGeneric { Success = true, Data = nFeEmission };
		}

		// Método principal para criação
		public async Task<ResponseGeneric> CreateAttemptAsync(NFeEmissionDto attempt)
		{
			var (validationResult, fiscalConfig, sale, naturezaOperacao) =
					await ValidateAndGetDependencies(attempt.CompanyId, attempt.SaleId, attempt.NaturezaOperacaoId);

			if (!validationResult.Success)
				return validationResult;

			NFeEmission ultimaNota = await (repository as INFeRepository).GetByCompany(attempt.CompanyId);
			int proximoNumeroNfe = CalculateNextNumber(ultimaNota, fiscalConfig);

			var respEmissao = await TransmitirNfe(proximoNumeroNfe, fiscalConfig, sale, naturezaOperacao);

			var entity = CreateNFeEmission(attempt, respEmissao, fiscalConfig, proximoNumeroNfe);

			await repository.CreateAsync(entity);
			return new ResponseGeneric { Success = true, Data = entity };
		}

		// Método privado para validações comuns
		private async Task<(ResponseGeneric ValidationResult, FiscalConfiguration FiscalConfig, Sale Sale, NaturezaOperacao NaturezaOperacao)>
				ValidateAndGetDependencies(int companyId, int saleId, int naturezaOperacaoId)
		{
			// Configuração da empresa
			FiscalConfiguration fiscalConfiguration = await _fiscalConfigurationRepository.GetByCompany(companyId);
			if (fiscalConfiguration == null)
				return (new ResponseGeneric { Success = false, Message = "Não encontrado as configurações para emissão de nota!" }, null, null, null);

			// Verifica se existe a venda
			Sale sale = await _saleRepository.GetSaleByCompany(saleId, companyId);
			if (sale == null)
				return (new ResponseGeneric { Success = false, Message = "Venda não encontrada para a empresa." }, null, null, null);

			if (sale.Financials == null || sale.Financials.Count() == 0)
				return (new ResponseGeneric { Success = false, Message = "Venda não possui financeiro!" }, null, null, null);

			// Natureza da operação
			NaturezaOperacao naturezaOperacao = await _naturezaOperacaoRepository.GetByIdAsync(naturezaOperacaoId);
			if (naturezaOperacao == null)
				return (new ResponseGeneric { Success = false, Message = "Natureza de operação não encontrada." }, null, null, null);

			return (new ResponseGeneric { Success = true }, fiscalConfiguration, sale, naturezaOperacao);
		}

		// Método privado para atualizar entidade existente
		private void UpdateNFeEmission(NFeEmission nFeEmission, object respEmissao, FiscalConfiguration fiscalConfig)
		{
			nFeEmission.Sent = true;
			nFeEmission.Serie = fiscalConfig.NumeracaoDocumentos.Nfce.Serie;
			nFeEmission.TryCount += 1;
			nFeEmission.UpdatedAt = DateTime.UtcNow;

			if (respEmissao is string mensagemErro)
			{
				nFeEmission.StatusNfe = StatusNfe.pendente;
				nFeEmission.ErrorMessage = mensagemErro;
				nFeEmission.ResponseJson = null;
			}
			else if (respEmissao is RetornoNFeAutorizacao ret)
			{
				var infProt = ret.Retorno.protNFe?.infProt;
				bool isAuthorized = infProt != null && NfeSituacao.Autorizada(infProt.cStat);

				nFeEmission.StatusNfe = isAuthorized ? StatusNfe.emitida : StatusNfe.pendente;
				nFeEmission.ResponseJson = JsonConvert.SerializeObject(infProt);
				nFeEmission.ErrorMessage = isAuthorized ? null : infProt?.xMotivo;
				nFeEmission.ChaveAcesso = infProt?.chNFe;
				nFeEmission.XmlCompleto = ret.Xml;
				nFeEmission.Protocolo = infProt?.nProt;
			}
		}

		// Método privado para criar nova entidade
		private NFeEmission CreateNFeEmission(NFeEmissionDto attempt, object respEmissao, FiscalConfiguration fiscalConfig, int numero)
		{
			var entity = new NFeEmission
			{
				NaturezaOperacaoId = attempt.NaturezaOperacaoId,
				SaleId = attempt.SaleId,
				TipoDocumento = attempt.TipoDocumento,
				Serie = fiscalConfig.NumeracaoDocumentos.Nfce.Serie,
				Numero = numero,
				CreatedAt = DateTime.UtcNow,
				TryCount = attempt.TryCount <= 0 ? 1 : attempt.TryCount,
				CompanyId = attempt.CompanyId
			};

			if (respEmissao is string mensagemErro)
			{
				entity.StatusNfe = StatusNfe.pendente;
				entity.ErrorMessage = mensagemErro;
				entity.ResponseJson = "";
			}
			else if (respEmissao is RetornoNFeAutorizacao ret)
			{
				var infProt = ret.Retorno.protNFe?.infProt;
				bool isAuthorized = infProt != null && NfeSituacao.Autorizada(infProt.cStat);

				entity.StatusNfe = isAuthorized ? StatusNfe.emitida : StatusNfe.pendente;
				entity.ResponseJson = JsonConvert.SerializeObject(infProt);
				entity.ErrorMessage = isAuthorized ? null : infProt?.xMotivo;
				entity.XmlCompleto = ret.Xml;
				entity.Protocolo = infProt?.nProt;
				entity.ChaveAcesso = infProt?.chNFe;
			}

			return entity;
		}

		// Método para calcular próximo número
		private int CalculateNextNumber(NFeEmission ultimaNota, FiscalConfiguration fiscalConfig)
		{
			if (ultimaNota == null)
				return Convert.ToInt32(fiscalConfig.NumeracaoDocumentos.Nfce.NumeroInicial);

			return Convert.ToInt32(ultimaNota.Numero + 1);
		}

		//     public async Task<ResponseGeneric> Resend(int id)
		//     {
		//         NFeEmission nFeEmission = await (repository as INFeRepository).GetByIdAsync(id);
		//         if (nFeEmission == null)
		//             return new ResponseGeneric { Success = false, Message = "Não foi encontrado a nota!" };
		//         //configuraçao da empresa para nfe
		//         FiscalConfiguration fiscalConfiguration = await _fiscalConfigurationRepository.GetByCompany(nFeEmission.CompanyId);
		//         if (fiscalConfiguration == null)
		//             return new ResponseGeneric { Success = false, Message = "Não encontrado as configurações para emissão de nota!" };
		//         //verifica se existe a venda
		//         Sale sale = await _saleRepository.GetSaleByCompany(nFeEmission.SaleId, nFeEmission.CompanyId);
		//         if (sale == null)
		//             return new ResponseGeneric { Success = false, Message = "Venda não encontrada para a empresa." };
		//         if (sale.Financials == null)
		//             return new ResponseGeneric { Success = false, Message = "Venda não possui financeiro!" };

		//         NaturezaOperacao naturezaOperacao = await _naturezaOperacaoRepository.GetByIdAsync(nFeEmission.NaturezaOperacaoId);
		//         if (naturezaOperacao == null)
		//             return new ResponseGeneric { Success = false, Message = "Natureza de operação não encontrada." };


		//         //classes externas para gerar nfe
		//         var respEmissao = await TransmitirNfe(Convert.ToInt32( nFeEmission.Numero), fiscalConfiguration, sale, naturezaOperacao);
		//         if (respEmissao is string mensagemErro)
		//         {
		//             //mudar status
		//             //mensagem de erro
		//             nFeEmission.Sent = true;
		//             nFeEmission.Numero = nFeEmission.Numero;
		//             nFeEmission.StatusNfe = StatusNfe.pendente;
		//             //nFeEmission.ResponseJson = responseJson;
		//             nFeEmission.ErrorMessage = respEmissao;
		//             nFeEmission.Serie = fiscalConfiguration.NumeracaoDocumentos.Nfce.Serie;
		//             nFeEmission.TryCount += 1;
		//             nFeEmission.UpdatedAt = DateTime.UtcNow;

		//         }
		//         else if (respEmissao is RetornoNFeAutorizacao ret)
		//         {
		//             protNFe protNFe = ret.Retorno.protNFe;
		//             infProt infProt = protNFe?.infProt;
		//             //mensagem de erro
		//             nFeEmission.Sent = true;
		//             nFeEmission.Numero = nFeEmission.Numero;
		//             nFeEmission.StatusNfe = NfeSituacao.Autorizada(infProt.cStat)? StatusNfe.emitida: StatusNfe.pendente;
		//             nFeEmission.ResponseJson = JsonConvert.SerializeObject (infProt);
		//             nFeEmission.ErrorMessage = NfeSituacao.Autorizada(infProt.cStat) ?null: infProt.xMotivo;
		//             nFeEmission.Serie = fiscalConfiguration.NumeracaoDocumentos.Nfce.Serie;
		//             nFeEmission.TryCount += 1;
		//             nFeEmission.UpdatedAt = DateTime.UtcNow;
		//             nFeEmission.ChaveAcesso = infProt.chNFe;
		//             nFeEmission.XmlCompleto = ret.Xml;
		//             nFeEmission.Protocolo = infProt.nProt;
		//         }

		//         await repository.UpdateAsync(nFeEmission.Id, nFeEmission);
		//         return new ResponseGeneric { Success = true,Data= nFeEmission };
		//     }
		//     public async Task<ResponseGeneric> CreateAttemptAsync(NFeEmissionDto attempt)
		//     {
		//         //ultima nota emitida com sucesso
		//         NFeEmission nFeEmission = await (repository as INFeRepository).GetByCompany(attempt.CompanyId);

		//         //configuraçao da empresa para nfe
		//         FiscalConfiguration fiscalConfiguration = await _fiscalConfigurationRepository.GetByCompany(attempt.CompanyId);
		//         if (fiscalConfiguration == null)
		//             return new ResponseGeneric { Success = false, Message = "Não encontrado as configurações para emissão de nota!" };

		//         //verifica se existe a venda
		//         Sale sale = await _saleRepository.GetSaleByCompany(attempt.SaleId, attempt.CompanyId);
		//         if (sale == null)
		//             return new ResponseGeneric { Success = false, Message = "Venda não encontrada para a empresa." };
		//         if (sale.Financials == null)
		//             return new ResponseGeneric { Success = false, Message = "Venda não possui financeiro!" };
		//if (sale.Financials == null)
		//	return new ResponseGeneric { Success = false, Message = "Venda não possui financeiro!" };
		//NaturezaOperacao naturezaOperacao = await _naturezaOperacaoRepository.GetByIdAsync(attempt.NaturezaOperacaoId);
		//         if (naturezaOperacao == null)
		//             return new ResponseGeneric { Success = false, Message = "Natureza de operação não encontrada." };
		//         int proximoNumeroNfe = Convert.ToInt32( nFeEmission == null ? fiscalConfiguration.NumeracaoDocumentos.Nfce.NumeroInicial : nFeEmission.Numero + 1);
		//         //classes externas para gerar nfe
		//         var respEmissao = await TransmitirNfe(proximoNumeroNfe, fiscalConfiguration, sale, naturezaOperacao);

		//         attempt.TryCount = attempt.TryCount <= 0 ? 1 : attempt.TryCount;
		//         attempt.CreatedAt = DateTime.UtcNow;
		//         var entity = new NFeEmission();
		//         if (respEmissao is string mensagemErro)
		//         {
		//             entity.ResponseJson = "";
		//             entity.ErrorMessage = mensagemErro;
		//             entity.NaturezaOperacaoId = attempt.NaturezaOperacaoId;
		//             entity.SaleId = attempt.SaleId;
		//             entity.TipoDocumento = attempt.TipoDocumento;
		//             entity.Serie = fiscalConfiguration.NumeracaoDocumentos.Nfce.Serie;
		//             entity.Numero = proximoNumeroNfe;
		//             entity.StatusNfe = StatusNfe.pendente;
		//             entity.CreatedAt = DateTime.UtcNow;
		//             entity.TryCount = attempt.TryCount;
		//             entity.CompanyId = attempt.CompanyId;
		//         }
		//         else if(respEmissao is RetornoNFeAutorizacao ret)
		//         {
		//             protNFe protNFe = ret.Retorno.protNFe;
		//             infProt infProt = protNFe?.infProt;
		//             entity.ResponseJson = JsonConvert.SerializeObject(infProt);
		//             entity.ErrorMessage= NfeSituacao.Autorizada(infProt.cStat) ? null : infProt.xMotivo;
		//             entity.NaturezaOperacaoId = attempt.NaturezaOperacaoId;
		//             entity.SaleId = attempt.SaleId;
		//             entity.TipoDocumento = attempt.TipoDocumento;
		//             entity.Serie = fiscalConfiguration.NumeracaoDocumentos.Nfce.Serie;
		//             entity.Numero = proximoNumeroNfe;
		//             entity.StatusNfe = NfeSituacao.Autorizada(infProt.cStat) ? StatusNfe.emitida : StatusNfe.pendente; ;
		//             entity.CreatedAt = DateTime.UtcNow;
		//             entity.TryCount = attempt.TryCount;
		//             entity.CompanyId = attempt.CompanyId;
		//             entity.XmlCompleto = ret.Xml;
		//             entity.Protocolo = infProt.nProt;
		//             entity.ChaveAcesso = infProt.chNFe;
		//         }


		//         await repository.CreateAsync(entity);
		//         return new ResponseGeneric { Success = true,Data = entity };
		//     }

		private async Task<dynamic> TransmitirNfe(int numero, FiscalConfiguration fiscalConfiguration, Sale sale, NaturezaOperacao naturezaOperacao)
		{
			try
			{
				//var numero = Funcoes.InpuBox(this, "Criar e Enviar NFe", "Número da Nota:");
				//if (string.IsNullOrEmpty(numero)) throw new Exception("O Número deve ser informado!");
				byte[] certbyte = await ObterCertificado(fiscalConfiguration.CertificadoDigital.Arquivo);
				_currentFiscalConfiguration = fiscalConfiguration;
				_currentNaturezaOperacao = naturezaOperacao;
				_currentSale = sale;
				_configuracaoApp = criarConfiguracaoApp(fiscalConfiguration, naturezaOperacao, certbyte);
				_nfe = ObterNfeValidada(VersaoServico.Versao400, ModeloDocumento.NFCe,
						numero, new ConfiguracaoCsc
						{
							CIdToken = fiscalConfiguration.Csc.Identificador,
							Csc = fiscalConfiguration.Csc.Valor
						});
				//_nfe.infNFeSupl.ObterUrlQrCode3();
				var xml = _nfe.ObterXmlString();
				var servicoNFe = new ServicosNFe(_configuracaoApp.CfgServico);
				Console.WriteLine("=== INICIANDO TRANSMISSÃO NFCe ===");
				Console.WriteLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
				Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("RENDER")}");
				var retornoEnvio = servicoNFe.NFeAutorizacao(int.Parse(fiscalConfiguration.NumeracaoDocumentos.Nfce.Serie), IndicadorSincronizacao.Sincrono, new List<NFe.Classes.NFe> { _nfe }, false/*Envia a mensagem compactada para a SEFAZ*/);
				/*             var resp=OnSucessoSync(retornoEnvio)*/
				;
				Console.WriteLine($"retorno: {retornoEnvio.Retorno.xMotivo}");

				//ExibeNfe();

				//var dlg = new Microsoft.Win32.SaveFileDialog
				//{
				//    FileName = _nfe.infNFe.Id.Substring(3),
				//    DefaultExt = ".xml",
				//    Filter = "Arquivo XML (.xml)|*.xml"
				//};
				//var result = dlg.ShowDialog();
				//if (result != true) return;
				//var arquivoXml = dlg.FileName;
				//_nfe.SalvarArquivoXml(arquivoXml);
				retornoEnvio.Xml = xml;

				return retornoEnvio;
			}
			catch (Exception ex)
			{
				//if (!string.IsNullOrEmpty(ex.Message))
				//    Funcoes.Mensagem(ex.Message, "Erro", MessageBoxButton.OK);
				return ex.Message;
			}
			finally
			{
				_currentFiscalConfiguration = null;
				_currentNaturezaOperacao = null;
				_currentSale = null;
			}
		}
		public static string LimparString(string texto, bool manterEspacos = false)
		{
			if (string.IsNullOrEmpty(texto))
				return texto;

			if (manterEspacos)
			{
				// Mantém letras, números e espaços
				return Regex.Replace(texto, @"[^a-zA-Z0-9\s]", "");
			}
			else
			{
				// Mantém apenas letras e números
				return Regex.Replace(texto, @"[^a-zA-Z0-9]", "");
			}
		}
		private string OnSucessoSync(RetornoBasico e)
		{
			//Console.Clear();
			//if (!string.IsNullOrEmpty(e.EnvioStr))
			//{
			//    Console.WriteLine("Xml Envio:");
			//    Console.WriteLine(FormatXml(e.EnvioStr) + "\n");
			//}

			//if (!string.IsNullOrEmpty(e.RetornoStr))
			//{
			//    Console.WriteLine("Xml Retorno:");
			//    Console.WriteLine(FormatXml(e.RetornoStr) + "\n");
			//}

			if (!string.IsNullOrEmpty(e.RetornoCompletoStr))
			{
				Console.WriteLine("Xml Retorno Completo:");
				Console.WriteLine(FormatXml(e.RetornoCompletoStr) + "\n");
			}
			return (FormatXml(e.RetornoCompletoStr) + "\n");
		}
		private static string FormatXml(string xml)
		{
			try
			{
				XDocument doc = XDocument.Parse(xml);
				return doc.ToString();
			}
			catch (Exception)
			{
				return xml;
			}
		}
		public async Task<byte[]> ObterCertificado(string caminhoRelativo)
		{
			try
			{

		
			// Extrai apenas o nome do arquivo do caminho salvo no banco
			// Exemplo: "/certs/399ff91c-fe15-43f3-b1cf-0d773e9f49cd.pfx" -> "399ff91c-fe15-43f3-b1cf-0d773e9f49cd.pfx"
			string nomeArquivo = Path.GetFileName(caminhoRelativo.TrimStart('/'));
			Console.WriteLine($"Nome do arquivo extraído: {nomeArquivo}");
			string caminhoCompleto;

			// Verifica se está no Render
			if (Environment.GetEnvironmentVariable("RENDER") == "true")
			{
				// NO RENDER: usa o caminho ABSOLUTO do Disk mount
				caminhoCompleto = Path.Combine("/app/wwwroot/certs", nomeArquivo);
				Console.WriteLine($"Caminho completo: {caminhoCompleto}");
			
			}
			else
			{
				// LOCAL: usa WebRootPath
				caminhoCompleto = Path.Combine(_environment.WebRootPath, "certs", nomeArquivo);
			}

			if (!System.IO.File.Exists(caminhoCompleto))
			{
				throw new FileNotFoundException(
						$"Certificado não encontrado. Procurado em: {caminhoCompleto}. " +
						$"Nome do arquivo: {nomeArquivo}. " +
						$"Caminho original: {caminhoRelativo}"
				);
			}

			return await System.IO.File.ReadAllBytesAsync(caminhoCompleto);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ERRO ao ler arquivo: {ex.Message}");
				Console.WriteLine($"StackTrace: {ex.StackTrace}");
				return [];
			}
		}
		private ConfiguracaoApp criarConfiguracaoApp(FiscalConfiguration fiscalConfiguration, NaturezaOperacao naturezaOperacao,
				byte[] certbyte)
		{
			try
			{

				var Certificado = new DFe.Utils.ConfiguracaoCertificado
				{
					TipoCertificado = DFe.Utils.TipoCertificado.A1ByteArray,
					ArrayBytesArquivo = certbyte,

					Senha = fiscalConfiguration.CertificadoDigital.Senha,
					ManterDadosEmCache = false,
					KeyStorageFlags = X509KeyStorageFlags.MachineKeySet |
		X509KeyStorageFlags.PersistKeySet |
		X509KeyStorageFlags.Exportable,
					SignatureMethodSignedXml = "http://www.w3.org/2000/09/xmldsig#rsa-sha1",
					DigestMethodReference = "http://www.w3.org/2000/09/xmldsig#sha1"

				};
				var ConfiguracaoEmail = new ConfiguracaoEmail();
				var ConfiguracaoCsc = new ConfiguracaoCsc
				{
					CIdToken = fiscalConfiguration.Csc.Identificador,
					Csc = fiscalConfiguration.Csc.Valor
				};
				var ConfiguracaoDanfeNfce = new NFe.Danfe.Base.NFCe.ConfiguracaoDanfeNfce
				{
					VersaoQrCode = VersaoQrCode.QrCodeVersao3
				};
				var DiretorioSchemas = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NFSchemas");
				var configuracaoApp = new ConfiguracaoApp
				{

					CfgServico = new ConfiguracaoServico
					{
						ModeloDocumento = naturezaOperacao.TipoDocumento == TipoDocumentoEnum.NFCE ? ModeloDocumento.NFCe : ModeloDocumento.NFe,
						VersaoNFeAutorizacao = VersaoServico.Versao400,
						VersaoNFeRetAutorizacao = VersaoServico.Versao400,
						VersaoLayout = VersaoServico.Versao400,
						cUF = (Estado)Enum.Parse(typeof(Estado), fiscalConfiguration.Emitente.EmitenteEndereco.Uf),
						ProtocoloDeSeguranca = System.Net.SecurityProtocolType.Tls12,
						VersaoConsultaGTIN = VersaoServico.Versao400,
						VersaoNfceAministracaoCSC = VersaoServico.Versao400,
						VersaoNfeDownloadNF = VersaoServico.Versao400,
						VersaoNfeRecepcao = VersaoServico.Versao400,
						ValidarSchemas = true,
						RemoverAcentos = true,
						DiretorioSchemas = DiretorioSchemas,
						tpAmb = fiscalConfiguration.Ambiente == AmbienteEnum.Homologacao ?
														TipoAmbiente.Homologacao : TipoAmbiente.Producao,
						tpEmis = TipoEmissao.teNormal,
						Certificado= Certificado
					},
					Emitente = new emit
					{
						CNPJ = LimparString(fiscalConfiguration.Emitente.Cnpj),
						IE = LimparString(fiscalConfiguration.Emitente.InscricaoEstadual),
						xNome = fiscalConfiguration.Emitente.RazaoSocial,
						xFant = fiscalConfiguration.Emitente.Fantasia,
						CRT = CRT.SimplesNacional,

					},

					EnderecoEmitente = new enderEmit
					{
						xLgr = fiscalConfiguration.Emitente.EmitenteEndereco.Logradouro,
						nro = fiscalConfiguration.Emitente.EmitenteEndereco.Numero,
						xCpl = fiscalConfiguration.Emitente.EmitenteEndereco.Complemento,
						xBairro = fiscalConfiguration.Emitente.EmitenteEndereco.Bairro,
						cMun = long.Parse(fiscalConfiguration.Emitente.EmitenteEndereco.CodigoCidade),
						xMun = fiscalConfiguration.Emitente.EmitenteEndereco.Cidade,
						UF = (Estado)Enum.Parse(typeof(Estado), fiscalConfiguration.Emitente.EmitenteEndereco.Uf),
						CEP = fiscalConfiguration.Emitente.EmitenteEndereco.Cep,
						fone = long.Parse(fiscalConfiguration.Emitente.EmitenteContato.Telefone)
					},
					ConfiguracaoCsc = ConfiguracaoCsc,
					ConfiguracaoDanfeNfce = ConfiguracaoDanfeNfce,
					EnviarTributacaoIbsCbs = false,
					EnviarTributacaoIS = false,
					ConfiguracaoEmail = ConfiguracaoEmail


				};
				return configuracaoApp;
			}
			catch (Exception ex)
			{

				throw;
			}
		}
		private NFe.Classes.NFe ObterNfeValidada(VersaoServico versaoServico, ModeloDocumento modelo, int numero,
				ConfiguracaoCsc configuracaoCsc)
		{
			var nfe = GetNf(numero, modelo, versaoServico);

			nfe.Assina();

			if (nfe.infNFe.ide.mod == ModeloDocumento.NFCe)
			{
				nfe.infNFeSupl = new infNFeSupl();
				if (versaoServico == VersaoServico.Versao400)
					nfe.infNFeSupl.urlChave = nfe.infNFeSupl.ObterUrlConsulta(nfe, _configuracaoApp.ConfiguracaoDanfeNfce.VersaoQrCode);
				nfe.infNFeSupl.qrCode = nfe.infNFeSupl.ObterUrlQrCode(nfe, _configuracaoApp.ConfiguracaoDanfeNfce.VersaoQrCode, configuracaoCsc.CIdToken, configuracaoCsc.Csc, _configuracaoApp.CfgServico.Certificado);
			}

			nfe.Valida();

			return nfe;
		}
		protected virtual NFe.Classes.NFe GetNf(int numero, ModeloDocumento modelo, VersaoServico versao)
		{
			var nf = new NFe.Classes.NFe { infNFe = GetInf(numero, modelo, versao) };
			return nf;
		}
		protected virtual infNFe GetInf(int numero, ModeloDocumento modelo, VersaoServico versao)
		{
			var infNFe = new infNFe
			{
				versao = versao.VersaoServicoParaString(),
				ide = GetIdentificacao(numero, modelo, versao),
				emit = GetEmitente(),
				dest = GetDestinatario(versao, modelo),
				transp = GetTransporte()
			};

			int index = 0;
			foreach (var i in _currentSale.SaleItems)
			{
				index++;
				infNFe.det.Add(GetDetalhe(index, i, infNFe.emit.CRT, modelo));
			}

			infNFe.total = GetTotal(versao, infNFe.det, modelo);

			if (infNFe.ide.mod == ModeloDocumento.NFe & (versao == VersaoServico.Versao310 || versao == VersaoServico.Versao400))
				infNFe.cobr = GetCobranca(infNFe.total.ICMSTot); //V3.00 e 4.00 Somente
			if (infNFe.ide.mod == ModeloDocumento.NFCe || (infNFe.ide.mod == ModeloDocumento.NFe & versao == VersaoServico.Versao400))
				infNFe.pag = GetPagamento(infNFe.total.ICMSTot, versao); //NFCe Somente  

			if (infNFe.ide.mod == ModeloDocumento.NFCe & versao != VersaoServico.Versao400)
				infNFe.infAdic = new infAdic() { infCpl = "" }; //Susgestão para impressão do troco em NFCe

			return infNFe;
		}
		//protected virtual List<pag> GetPagamento(ICMSTot icmsTot, VersaoServico versao)
		//{
		//    var valorPagto = (icmsTot.vNF / 2).Arredondar(2);

		//    if (versao != VersaoServico.Versao400) // difernte de versão 4 retorna isso
		//    {
		//        var p = new List<pag>
		//        {
		//            new pag {tPag = FormaPagamento.fpDinheiro, vPag = valorPagto},
		//            new pag {tPag = FormaPagamento.fpCheque, vPag = icmsTot.vNF - valorPagto}
		//        };
		//        return p;
		//    }


		//    // igual a versão 4 retorna isso
		//    var p4 = new List<pag>
		//    {
		//        //new pag {detPag = new detPag {tPag = FormaPagamento.fpDinheiro, vPag = valorPagto}},
		//        //new pag {detPag = new detPag {tPag = FormaPagamento.fpCheque, vPag = icmsTot.vNF - valorPagto}}
		//        new pag
		//        {
		//            detPag = new List<detPag>
		//            {
		//                new detPag {tPag = FormaPagamento.fpDinheiro, vPag = valorPagto},
		//                new detPag {tPag = FormaPagamento.fpCheque, vPag = icmsTot.vNF - valorPagto}
		//            }
		//        }
		//    };


		//    return p4;
		//}
		protected virtual List<pag> GetPagamento(ICMSTot icmsTot, VersaoServico versao)
		{
			if (_currentSale.Financials == null || !_currentSale.Financials.Any())
				return null;

			var pagamentos = _currentSale.Financials
					.Where(f => f.FinancialType == FinancialType.recipe &&
										 f.FinancialStatus != FinancialStatus.Canceled)
					.ToList();

			if (!pagamentos.Any())
				return null;

			decimal totalPagamentos = pagamentos.Sum(f => f.Value);
			decimal totalNF = icmsTot.vNF;

			// Validação: o total dos pagamentos deve ser igual ao total da NF
			if (Math.Abs(totalPagamentos - totalNF) > 0.01m)
			{
				// Log de aviso ou ajuste automático
				// Pode-se ajustar o último pagamento para igualar
				var ultimoPagamento = pagamentos.Last();
				ultimoPagamento.Value = totalNF - (totalPagamentos - ultimoPagamento.Value);
			}

			// Versão 3.10 ou inferior
			if (versao != VersaoServico.Versao400)
			{
				return pagamentos.Select(f => new pag
				{
					tPag = ConverterPorNome(f.PaymentMethod.Name),
					vPag = Math.Round(f.Value, 2),

					//		 xPag = (ConverterParaFormaPagamento(f.PaymentMethod) == FormaPagamento.fpOutro)
					//? f.PaymentMethod?.Name ?? "Outros"
					//: null
				}).ToList();
			}

			// Versão 4.00
			// Versão 4.00
			var pagamentosV4 = new pag
			{
				detPag = pagamentos.Select(f =>
				{
					var formaPagamento = ConverterPorNome(f.PaymentMethod.Name);
					var detPagObj = new detPag
					{
						tPag = formaPagamento,
						vPag = Math.Round(f.Value, 2)
					};

					// SÓ adicionar dados do cartão se for cartão de crédito OU cartão de débito
					if (formaPagamento == FormaPagamento.fpCartaoCredito ||
							formaPagamento == FormaPagamento.fpCartaoDebito)
					{
						detPagObj.card = new card
						{
							tpIntegra = TipoIntegracaoPagamento.TipNaoIntegrado,
							cAut = "NAOINTEGRADO",
							CNPJ = "00000000000000",
							tBand = BandeiraCartao.bcOutros
						};
					}

					// Adicionar descrição se for "Outros" (99)
					if (formaPagamento == FormaPagamento.fpOutro)
					{
						detPagObj.xPag = f.PaymentMethod?.Name ?? "Outros";
					}

					// Para PIX, não precisa de dados adicionais
					// Para Dinheiro, Cheque, etc., também não precisa

					return detPagObj;
				}).ToList()
			};
			return new List<pag> { pagamentosV4 };
		}
		private BandeiraCartao? ConverterParaBandeiraCartao(string cardBrand)
		{
			if (string.IsNullOrEmpty(cardBrand))
				return null;

			switch (cardBrand.ToUpper())
			{
				case "VISA":
				case "VISA ELECTRON":
					return BandeiraCartao.bcVisa;

				case "MASTERCARD":
				case "MASTERCARD MAESTRO":
					return BandeiraCartao.bcMasterCard;

				case "AMEX":
				case "AMERICAN EXPRESS":
					return BandeiraCartao.bcAmericanExpress;

				case "ELO":
					return BandeiraCartao.Elo;

				case "HIPERCARD":
					return BandeiraCartao.Hipercard;

				case "DINERS":
				case "DINERS CLUB":
					return BandeiraCartao.bcDinersClub;

				case "JCB":
					return BandeiraCartao.JcB;

				case "CREDENCIAL":
					return BandeiraCartao.Credz;

				case "OUTROS":
				case "OUTRA":
				default:
					return BandeiraCartao.bcOutros;
			}
		}
		private FormaPagamento ConverterPorNome(string nome)
		{
			if (string.IsNullOrEmpty(nome))
				return FormaPagamento.fpOutro;

			var nomeUpper = nome.ToUpper().Trim();

			if (nomeUpper.Contains("PIX"))
				return FormaPagamento.fpOutro;

			if (nomeUpper.Contains("DINHEIRO"))
				return FormaPagamento.fpDinheiro;

			if (nomeUpper.Contains("CHEQUE"))
				return FormaPagamento.fpCheque;

			if (nomeUpper.Contains("CRÉDITO") || nomeUpper.Contains("CREDITO"))
				return FormaPagamento.fpCartaoCredito;

			if (nomeUpper.Contains("DÉBITO") || nomeUpper.Contains("DEBITO"))
				return FormaPagamento.fpCartaoDebito;
			if (nomeUpper.Contains("BOLETO") || nomeUpper.Contains("DUPLICATA"))
				return FormaPagamento.fpBoletoBancario;

			return FormaPagamento.fpOutro;
		}
		//private FormaPagamento ConverterParaFormaPagamento(PaymentMethod paymentMethod)
		//      {
		//          // Mapeamento baseado no nome ou ID do método de pagamento
		//          switch (paymentMethod.Name?.ToUpper())
		//          {

		//              case "DINHEIRO":
		//                  return FormaPagamento.fpDinheiro;

		//              case "CHEQUE":
		//                  return FormaPagamento.fpCheque;

		//              case "CARTÃO DE CRÉDITO":
		//              case "CARTAO CREDITO":
		//              case "CREDITO":
		//              case "CARTAO":
		//              case "CARTÃO":
		//                  return FormaPagamento.fpCartaoCredito;

		//              case "CARTÃO DE DÉBITO":
		//              case "CARTAO DEBITO":
		//              case "DEBITO":
		//                  return FormaPagamento.fpCartaoDebito;

		//              //case "CRÉDITO LOJA":
		//              //case "CREDITO LOJA":
		//              //    return FormaPagamento.fpCreditoLoja;

		//              case "VALE ALIMENTAÇÃO":
		//              case "VALE ALIMENTACAO":
		//                  return FormaPagamento.fpValeAlimentacao;

		//              case "VALE REFEIÇÃO":
		//              case "VALE REFEICAO":
		//                  return FormaPagamento.fpValeRefeicao;

		//              case "VALE PRESENTE":
		//                  return FormaPagamento.fpValePresente;

		//              case "VALE COMBUSTÍVEL":
		//              case "VALE COMBUSTIVEL":
		//                  return FormaPagamento.fpValeCombustivel;

		//              case "BOLETO":
		//              case "DUPLICATA":
		//                  return FormaPagamento.fpBoletoBancario;

		//              case "SEM PAGAMENTO":
		//                  return FormaPagamento.fpSemPagamento;

		//              case "PIX":
		//                  return FormaPagamento.fpPagamentoInstantaneoPIXDinamico;

		//              default:
		//                  return FormaPagamento.fpOutro;
		//          }
		//      }

		protected virtual cobr GetCobranca(ICMSTot icmsTot)
		{
			if (_currentSale.Financials == null || !_currentSale.Financials.Any())
				return null;

			var financials = _currentSale.Financials
					.Where(f => f.FinancialType == FinancialType.recipe &&
										 f.FinancialStatus != FinancialStatus.Canceled)
					.OrderBy(f => f.DueDate)
					.ToList();

			if (!financials.Any())
				return null;

			decimal totalParcelas = financials.Sum(f => f.Value);
			decimal totalNF = icmsTot.vNF;

			// Validação: a soma das parcelas deve ser igual ao total da NF
			// (considerando uma pequena margem de erro por arredondamento)
			if (Math.Abs(totalParcelas - totalNF) > 0.01m)
			{
				// Log de aviso ou ajuste automático
				// Pode-se ajustar a última parcela para igualar
				var ultimaParcela = financials.Last();
				ultimaParcela.Value = totalNF - (totalParcelas - ultimaParcela.Value);
			}

			var duplicatas = new List<dup>();
			int parcelaNumero = 1;

			foreach (var financial in financials)
			{
				duplicatas.Add(new dup
				{
					nDup = parcelaNumero.ToString().PadLeft(3, '0'),
					dVenc = financial.DueDate,
					vDup = Math.Round(financial.Value, 2)
				});
				parcelaNumero++;
			}

			return new cobr
			{
				dup = duplicatas
			};
		}
		//protected virtual cobr GetCobranca(ICMSTot icmsTot)
		//{

		//    var valorParcela = (icmsTot.vNF / 2).Arredondar(2);
		//    var c = new cobr
		//    {
		//        fat = new fat { nFat = "12345678910", vLiq = icmsTot.vNF, vOrig = icmsTot.vNF, vDesc = 0m },
		//        dup = new List<dup>
		//        {
		//            new dup {nDup = "001", dVenc = DateTime.Now.AddDays(30), vDup = valorParcela},
		//            new dup {nDup = "002", dVenc = DateTime.Now.AddDays(60), vDup = icmsTot.vNF - valorParcela}
		//        }
		//    };

		//    return c;
		//}
		private bool ValidarGTIN(string gtin)
		{
			if (string.IsNullOrEmpty(gtin))
				return false;

			// Remove espaços e caracteres não numéricos
			gtin = new string(gtin.Where(char.IsDigit).ToArray());

			// Verifica se tem 8, 12, 13 ou 14 dígitos
			if (gtin.Length != 8 && gtin.Length != 12 && gtin.Length != 13 && gtin.Length != 14)
				return false;

			// Validação básica - não pode começar com zero
			if (gtin.StartsWith("0"))
				return false;

			// Valida dígito verificador (opcional, mas recomendado)
			return ValidarDigitoVerificadorGTIN(gtin);
		}

		private bool ValidarDigitoVerificadorGTIN(string gtin)
		{
			if (string.IsNullOrEmpty(gtin))
				return false;

			int[] pesos;
			int digitoVerificador = int.Parse(gtin.Last().ToString());
			string semDigito = gtin.Substring(0, gtin.Length - 1);

			// Define pesos baseado no tamanho
			if (gtin.Length == 13) // GTIN-13 (EAN-13)
			{
				pesos = new int[] { 1, 3, 1, 3, 1, 3, 1, 3, 1, 3, 1, 3 };
			}
			else if (gtin.Length == 12) // GTIN-12 (UPC-A)
			{
				pesos = new int[] { 3, 1, 3, 1, 3, 1, 3, 1, 3, 1, 3 };
			}
			else
			{
				return true; // Para outros tamanhos, não valida
			}

			int soma = 0;
			for (int i = 0; i < semDigito.Length; i++)
			{
				soma += int.Parse(semDigito[i].ToString()) * pesos[i];
			}

			int resto = soma % 10;
			int digitoCalculado = resto == 0 ? 0 : 10 - resto;

			return digitoCalculado == digitoVerificador;
		}
		protected virtual prod GetProduto(SaleItems i)
		{
			string gtin = i.Product.Code ?? "";
			bool isGtinValido = ValidarGTIN(gtin);
			var p = new prod
			{
				cProd = i.Product.Id.ToString().PadLeft(5, '0'),
				cEAN = isGtinValido ? gtin : "SEM GTIN",
				xProd = _currentFiscalConfiguration.Ambiente == AmbienteEnum.Homologacao
						? "NOTA FISCAL EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL"  // Texto padrão para produto
						: i.Product.Name,
				NCM = i.Product.Ncm,
				CFOP = int.Parse(_currentNaturezaOperacao.Cfop),
				uCom = "UNID",
				qCom = i.Amount,
				vUnCom = i.Value,
				vProd = (i.Value * i.Amount),
				vDesc = 0,
				cEANTrib = isGtinValido ? gtin : "SEM GTIN",
				uTrib = "UNID",
				qTrib = i.Amount,
				vUnTrib = i.Value,
				indTot = IndicadorTotal.ValorDoItemCompoeTotalNF,
				//NVE = {"AA0001", "AB0002", "AC0002"},
				//CEST = ?

				//ProdutoEspecifico = new arma
				//{
				//    tpArma = TipoArma.UsoPermitido,
				//    nSerie = "123456",
				//    nCano = "123456",
				//    descr = "TESTE DE ARMA"
				//}
			};
			return p;
		}
		/*protected virtual det GetDetalhe(int index,SaleItems i, CRT crt, ModeloDocumento modelo)
		{
				var produto = GetProduto(i);

				var det = new det
				{
						nItem = index,
						prod = produto,
						imposto = new imposto
						{
								vTotTrib = 0.17m,

								ICMS = new ICMS
								{
										//Se você já tem os dados de toda a tributação persistida no banco em uma única tabela, utilize a linha comentada abaixo para preencher as tags do ICMS
										//TipoICMS = ObterIcmsBasico(crt),

										//Caso você resolva utilizar método ObterIcmsBasico(), comente esta proxima linha
										TipoICMS =
												crt == CRT.SimplesNacional || crt == CRT.SimplesNacionalMei
														? InformarCSOSN(Csosnicms.Csosn102)
														: InformarICMS(Csticms.Cst00, VersaoServico.Versao310)
								},

								//ICMSUFDest = new ICMSUFDest()
								//{
								//    pFCPUFDest = 0,
								//    pICMSInter = 12,
								//    pICMSInterPart = 0,
								//    pICMSUFDest = 0,
								//    vBCUFDest = 0,
								//    vFCPUFDest = 0,
								//    vICMSUFDest = 0,
								//    vICMSUFRemet = 0
								//},

								COFINS = new COFINS
								{
										//Se você já tem os dados de toda a tributação persistida no banco em uma única tabela, utilize a linha comentada abaixo para preencher as tags do COFINS
										//TipoCOFINS = ObterCofinsBasico(),

										//Caso você resolva utilizar método ObterCofinsBasico(), comente esta proxima linha

										TipoCOFINS = !_currentNaturezaOperacao.ConfiguracaoTributaria.AplicarCOFINS ?
										new COFINSOutr { CST = CSTCOFINS.cofins99,
												pCOFINS = 0, vBC = 0, vCOFINS = 0 }:
												new COFINSOutr
												{
														CST = (CSTCOFINS) Enum.Parse( typeof(CSTCOFINS), _currentNaturezaOperacao.ConfiguracaoTributaria.CstCOFINS),
														pCOFINS = _currentNaturezaOperacao.ConfiguracaoTributaria.AliquotaCOFINS,
														vBC = i.Value * i.Amount,
														vCOFINS = 0
												}

								},

								PIS = new PIS
								{
										//Se você já tem os dados de toda a tributação persistida no banco em uma única tabela, utilize a linha comentada abaixo para preencher as tags do PIS
										//TipoPIS = ObterPisBasico(),

										//Caso você resolva utilizar método ObterPisBasico(), comente esta proxima linha
										TipoPIS = new PISOutr { CST = CSTPIS.pis99, pPIS = 0, vBC = 0, vPIS = 0 }
								},

								//IS = (modelo == ModeloDocumento.NFe && _currentNaturezaOperacao.ConfiguracaoTributaria.AplicarIS == true) ? new IS
								//{
								//    cClassTribIS = "000001",
								//    uTrib = "UN",
								//    qTrib = 1,
								//    CSTIS = "000",
								//    pIS = 0,
								//    vIS = 0
								//} : null,

								//IBSCBS =(modelo == ModeloDocumento.NFe && _currentNaturezaOperacao.ConfiguracaoTributaria.AplicarIBS == true) ? new IBSCBS
								//{
								//    CST = CSTIBSCBS.cst000,
								//    cClassTrib = "000001",
								//    gIBSCBS = new gIBSCBS
								//    {
								//        vBC = 0,
								//        gIBSUF = new gIBSUF
								//        {
								//            pIBSUF = 0.10m,
								//            vIBSUF = 0,
								//        },
								//        gIBSMun = new gIBSMun
								//        {
								//            pIBSMun = 0,
								//            vIBSMun = 0,
								//        },
								//        gCBS = new gCBS
								//        {
								//            pCBS = 0.90m,
								//            vCBS = 0,
								//        },
								//        vIBS = 0// opcional
								//    }
								//} : null
						}
				};

				if (modelo == ModeloDocumento.NFe) //NFCe não aceita grupo "IPI"
				{
						det.imposto.IPI = new IPI()
						{
								cEnq = 999,

								//Se você já tem os dados de toda a tributação persistida no banco em uma única tabela, utilize a linha comentada abaixo para preencher as tags do IPI
								//TipoIPI = ObterIPIBasico(),

								//Caso você resolva utilizar método ObterIPIBasico(), comente esta proxima linha
								TipoIPI = new IPITrib() { CST = CSTIPI.ipi00, pIPI = 5, vBC = 1, vIPI = 0.05m }
						};
				}

				//det.impostoDevol = new impostoDevol() { IPI = new IPIDevolvido() { vIPIDevol = 10 }, pDevol = 100 };

				return det;
		}*/
		protected virtual det GetDetalhe(int index, SaleItems i, CRT crt, ModeloDocumento modelo)
		{
			var produto = GetProduto(i);
			decimal valorTotalItem = i.Value * i.Amount;
			decimal aliquotaCOFINS = _currentNaturezaOperacao.ConfiguracaoTributaria.AliquotaCOFINS;
			decimal aliquotaPIS = _currentNaturezaOperacao.ConfiguracaoTributaria.AliquotaPIS;
			decimal aliquotaIPI = _currentNaturezaOperacao.ConfiguracaoTributaria.AliquotaIPI;
			//string cstPIS = _currentNaturezaOperacao.ConfiguracaoTributaria.CstPIS;

			//// Validar se o CST é válido, se não for, usar 99
			//if (!Enum.IsDefined(typeof(CSTPIS),int.Parse( cstPIS)))
			//{
			//	cstPIS = "99"; // Default para outras operações
			//}
			var det = new det
			{
				nItem = index,
				prod = produto,
				imposto = new imposto
				{
					vTotTrib = CalcularTotalTributos(i, aliquotaCOFINS, aliquotaPIS, aliquotaIPI),

					ICMS = new ICMS
					{
						TipoICMS =
														crt == CRT.SimplesNacional || crt == CRT.SimplesNacionalMei
																? InformarCSOSN(Csosnicms.Csosn102)
																: InformarICMS(Csticms.Cst00, VersaoServico.Versao310)
					},

					COFINS = new COFINS
					{
						TipoCOFINS = !_currentNaturezaOperacao.ConfiguracaoTributaria.AplicarCOFINS ?
														new COFINSOutr
														{
															CST = CSTCOFINS.cofins99,
															pCOFINS = 0,
															vBC = valorTotalItem,
															vCOFINS = 0
														} :
														new COFINSOutr
														{
															CST = (CSTCOFINS)Enum.Parse(typeof(CSTCOFINS), _currentNaturezaOperacao.ConfiguracaoTributaria.CstCOFINS),
															pCOFINS = aliquotaCOFINS,
															vBC = valorTotalItem,
															vCOFINS = CalcularValorCOFINS(valorTotalItem, aliquotaCOFINS)
														}
					},

					PIS = new PIS
					{
						TipoPIS = !_currentNaturezaOperacao.ConfiguracaoTributaria.AplicarPIS ?
														new PISOutr
														{
															CST = CSTPIS.pis99,
															pPIS = 0,
															vBC = valorTotalItem,
															vPIS = 0
														} :
														new PISOutr
														{
															CST = (CSTPIS)Enum.Parse(typeof(CSTPIS), _currentNaturezaOperacao.ConfiguracaoTributaria.CstPIS),
															pPIS = aliquotaPIS,
															vBC = valorTotalItem,
															vPIS = CalcularValorPIS(valorTotalItem, aliquotaPIS)
														}
					}
				}
			};

			if (modelo == ModeloDocumento.NFe) //NFCe não aceita grupo "IPI"
			{
				det.imposto.IPI = new IPI()
				{
					cEnq = 999,

					//Se você já tem os dados de toda a tributação persistida no banco em uma única tabela, utilize a linha comentada abaixo para preencher as tags do IPI
					//TipoIPI = ObterIPIBasico(),

					//Caso você resolva utilizar método ObterIPIBasico(), comente esta proxima linha
					TipoIPI = new IPITrib() { CST = CSTIPI.ipi00, pIPI = 5, vBC = 1, vIPI = 0.05m }
				};
			}

			return det;
		}

		// Métodos auxiliares
		private decimal CalcularTotalTributos(SaleItems item, decimal aliquotaCOFINS, decimal aliquotaPIS, decimal aliquotaIPI)
		{
			decimal valorTotalItem = item.Value * item.Amount;
			decimal totalTributos = 0;

			// Adicionar COFINS se aplicável
			if (_currentNaturezaOperacao.ConfiguracaoTributaria.AplicarCOFINS && aliquotaCOFINS > 0)
			{
				totalTributos += valorTotalItem * aliquotaCOFINS / 100;
			}

			// Adicionar PIS se aplicável
			if (_currentNaturezaOperacao.ConfiguracaoTributaria.AplicarPIS && aliquotaPIS > 0)
			{
				totalTributos += valorTotalItem * aliquotaPIS / 100;
			}

			// Adicionar IPI se aplicável (apenas para NFe)
			if (_currentNaturezaOperacao.ConfiguracaoTributaria.AplicarIPI && aliquotaIPI > 0)
			{
				totalTributos += valorTotalItem * aliquotaIPI / 100;
			}

			// Adicionar ICMS aproximado se disponível
			var icms = CalcularICMSAproximado(item);
			if (icms > 0)
			{
				totalTributos += icms;
			}

			return Math.Round(totalTributos, 2);
		}

		private decimal CalcularValorCOFINS(decimal valorTotalItem, decimal aliquota)
		{
			if (!_currentNaturezaOperacao.ConfiguracaoTributaria.AplicarCOFINS || aliquota <= 0)
				return 0;

			return Math.Round(valorTotalItem * aliquota / 100, 2);
		}

		private decimal CalcularValorPIS(decimal valorTotalItem, decimal aliquota)
		{
			if (!_currentNaturezaOperacao.ConfiguracaoTributaria.AplicarPIS || aliquota <= 0)
				return 0;

			return Math.Round(valorTotalItem * aliquota / 100, 2);
		}

		private decimal CalcularValorIPI(decimal valorTotalItem, decimal aliquota)
		{
			if (!_currentNaturezaOperacao.ConfiguracaoTributaria.AplicarIPI || aliquota <= 0)
				return 0;

			return Math.Round(valorTotalItem * aliquota / 100, 2);
		}

		private decimal CalcularICMSAproximado(SaleItems item)
		{
			// Implementar cálculo do ICMS aproximado baseado na configuração
			// Este é um valor aproximado para o vTotTrib
			decimal valorTotalItem = item.Value * item.Amount;
			decimal aliquotaICMS = 18; // Alíquota padrão, ajuste conforme necessidade

			// Verificar se é Simples Nacional
			if (_configuracaoApp.Emitente.CRT == CRT.SimplesNacional)
			{
				// Para Simples Nacional, usar alíquota aproximada do produto
				aliquotaICMS = 7; // Exemplo: 7% para Simples Nacional
			}

			return Math.Round(valorTotalItem * aliquotaICMS / 100, 2);
		}
		protected virtual ICMSBasico InformarICMS(Csticms CST, VersaoServico versao)
		{
			var icms20 = new ICMS20
			{
				orig = OrigemMercadoria.OmNacional,
				CST = Csticms.Cst20,
				modBC = DeterminacaoBaseIcms.DbiValorOperacao,
				vBC = 1.1m,
				pICMS = 18,
				vICMS = 0.20m,
				motDesICMS = MotivoDesoneracaoIcms.MdiTaxi
			};
			if (versao == VersaoServico.Versao310)
				icms20.vICMSDeson = 0.10m; //V3.00 ou maior Somente

			switch (CST)
			{
				case Csticms.Cst00:
					return new ICMS00
					{
						CST = Csticms.Cst00,
						modBC = DeterminacaoBaseIcms.DbiValorOperacao,
						orig = OrigemMercadoria.OmNacional,
						pICMS = 18,
						vBC = 1.1m,
						vICMS = 0.20m
					};
				case Csticms.Cst20:
					return icms20;
					//Outros casos aqui
			}

			return new ICMS10();
		}
		protected virtual ICMSBasico InformarCSOSN(Csosnicms CST)
		{
			switch (CST)
			{
				case Csosnicms.Csosn101:
					return new ICMSSN101
					{
						CSOSN = Csosnicms.Csosn101,
						orig = OrigemMercadoria.OmNacional
					};
				case Csosnicms.Csosn102:
					return new ICMSSN102
					{
						CSOSN = Csosnicms.Csosn102,
						orig = OrigemMercadoria.OmNacional
					};
				//Outros casos aqui
				default:
					return new ICMSSN201();
			}
		}
		protected virtual total GetTotal(VersaoServico versao, List<det> produtos, ModeloDocumento modeloDocumento)
		{
			var icmsTot = new ICMSTot
			{
				vProd = produtos.Sum(p => p.prod.vProd),
				vDesc = produtos.Sum(p => p.prod.vDesc ?? 0),
				vTotTrib = produtos.Sum(p => p.imposto.vTotTrib ?? 0),
			};

			if (versao == VersaoServico.Versao310 || versao == VersaoServico.Versao400)
				icmsTot.vICMSDeson = 0;

			if (versao == VersaoServico.Versao400)
			{
				icmsTot.vFCPUFDest = 0;
				icmsTot.vICMSUFDest = 0;
				icmsTot.vICMSUFRemet = 0;
				icmsTot.vFCP = 0;
				icmsTot.vFCPST = 0;
				icmsTot.vFCPSTRet = 0;
				icmsTot.vIPIDevol = 0;
			}

			foreach (var produto in produtos)
			{
				if (produto.imposto.IPI != null && produto.imposto.IPI.TipoIPI.GetType() == typeof(IPITrib))
					icmsTot.vIPI = icmsTot.vIPI + ((IPITrib)produto.imposto.IPI.TipoIPI).vIPI ?? 0;
				if (produto.imposto.ICMS.TipoICMS.GetType() == typeof(ICMS00))
				{
					icmsTot.vBC = icmsTot.vBC + ((ICMS00)produto.imposto.ICMS.TipoICMS).vBC;
					icmsTot.vICMS = icmsTot.vICMS + ((ICMS00)produto.imposto.ICMS.TipoICMS).vICMS;
				}
				if (produto.imposto.ICMS.TipoICMS.GetType() == typeof(ICMS20))
				{
					icmsTot.vBC = icmsTot.vBC + ((ICMS20)produto.imposto.ICMS.TipoICMS).vBC;
					icmsTot.vICMS = icmsTot.vICMS + ((ICMS20)produto.imposto.ICMS.TipoICMS).vICMS;
				}
				//Outros Ifs aqui, caso vá usar as classes ICMS00, ICMS10 para totalizar
			}

			//** Regra de validação W16-10 que rege sobre o Total da NF **//
			icmsTot.vNF =
					icmsTot.vProd
					- icmsTot.vDesc
					- icmsTot.vICMSDeson.GetValueOrDefault()
					+ icmsTot.vST
					+ icmsTot.vFCPST.GetValueOrDefault()
					+ icmsTot.vFrete
					+ icmsTot.vSeg
					+ icmsTot.vOutro
					+ icmsTot.vII
					+ icmsTot.vIPI
					+ icmsTot.vIPIDevol.GetValueOrDefault();

			var t = new total
			{
				ICMSTot = icmsTot,
				//IBSCBSTot = (modeloDocumento == ModeloDocumento.NFe && _currentNaturezaOperacao.ConfiguracaoTributaria.AplicarIBS == true) ? new IBSCBSTot
				//{
				//    vBCIBSCBS = 0,
				//    gIBS = new gIBSTotal
				//    {
				//        gIBSUF = new gIBSUFTotal
				//        {
				//            vDif = 0,
				//            vDevTrib = 0,
				//            vIBSUF = 0
				//        },
				//        gIBSMun = new gIBSMunTotal
				//        {
				//            vDif = 0,
				//            vDevTrib = 0,
				//            vIBSMun = 0
				//        },
				//        vIBS = 0,
				//        vCredPres = 0,
				//        vCredPresCondSus = 0
				//    },
				//    gCBS = new gCBSTotal
				//    {
				//        vDif = 0,
				//        vDevTrib = 0,
				//        vCBS = 0,
				//        vCredPres = 0,
				//        vCredPresCondSus = 0
				//    }
				//} : null,
				//ISTot =(modeloDocumento == ModeloDocumento.NFe&& _currentNaturezaOperacao.ConfiguracaoTributaria.AplicarIS == true) ? new ISTot()
				//{
				//    vIS = 0
				//} : null
			};
			return t;
		}
		protected virtual ide GetIdentificacao(int numero, ModeloDocumento modelo, VersaoServico versao)
		{
			var ide = new ide
			{
				cUF = _configuracaoApp.EnderecoEmitente.UF,
				natOp = _currentNaturezaOperacao.Descricao,
				mod = modelo,
				serie = int.Parse(_currentFiscalConfiguration.NumeracaoDocumentos.Nfce.Serie),
				nNF = numero,
				tpNF = TipoNFe.tnSaida,
				cMunFG = _configuracaoApp.EnderecoEmitente.cMun,
				tpEmis = _configuracaoApp.CfgServico.tpEmis,
				tpImp = TipoImpressao.tiRetrato,
				cNF = "1234",
				tpAmb = _configuracaoApp.CfgServico.tpAmb,
				finNFe = FinalidadeNFe.fnNormal,
				verProc = "3.000",
				indIntermed = IndicadorIntermediador.iiSemIntermediador
			};

			if (ide.tpEmis != TipoEmissao.teNormal)
			{
				ide.dhCont = DateTime.Now;
				ide.xJust = "CONTIGÊNCIA PARA NFe/NFCe";
			}

			#region V2.00

			if (versao == VersaoServico.Versao200)
			{
				ide.dEmi = DateTime.Today; //Mude aqui para enviar a nfe vinculada ao EPEC, V2.00
				ide.dSaiEnt = DateTime.Today;
			}

			#endregion

			#region V3.00

			if (versao == VersaoServico.Versao200) return ide;

			if (versao == VersaoServico.Versao310)
			{
				ide.indPag = IndicadorPagamento.ipVista;
			}


			ide.idDest = DestinoOperacao.doInterna;
			ide.dhEmi = DateTime.Now;
			//Mude aqui para enviar a nfe vinculada ao EPEC, V3.10
			if (ide.mod == ModeloDocumento.NFe)
				ide.dhSaiEnt = DateTime.Now;
			else
				ide.tpImp = TipoImpressao.tiNFCe;
			ide.procEmi = ProcessoEmissao.peAplicativoContribuinte;
			ide.indFinal = ConsumidorFinal.cfConsumidorFinal; //NFCe: Tem que ser consumidor Final
			ide.indPres = PresencaComprador.pcPresencial; //NFCe: deve ser 1 ou 4

			#endregion

			return ide;
		}

		protected virtual emit GetEmitente()
		{
			var emit = _configuracaoApp.Emitente; // new emit
																						//{
																						//    //CPF = "12345678912",
																						//    CNPJ = "12345678000189",
																						//    xNome = "RAZAO SOCIAL LTDA",
																						//    xFant = "FANTASIA LTRA",
																						//    IE = "123456789",
																						//};
			emit.enderEmit = GetEnderecoEmitente();
			return emit;
		}

		protected virtual enderEmit GetEnderecoEmitente()
		{
			var enderEmit = _configuracaoApp.EnderecoEmitente; // new enderEmit
																												 //{
																												 //    xLgr = "RUA TESTE DE ENREREÇO",
																												 //    nro = "123",
																												 //    xCpl = "1 ANDAR",
																												 //    xBairro = "CENTRO",
																												 //    cMun = 2802908,
																												 //    xMun = "ITABAIANA",
																												 //    UF = "SE",
																												 //    CEP = 49500000,
																												 //    fone = 79123456789
																												 //};
			enderEmit.cPais = 1058;
			enderEmit.xPais = "BRASIL";
			return enderEmit;
		}
		protected virtual dest GetDestinatario(VersaoServico versao, ModeloDocumento modelo)
		{
			var dest = new dest(versao);

			// Configuração do documento (CPF ou CNPJ)
			if (_currentSale?.Client?.TipoPessoa == "J")
			{
				dest.CNPJ = _currentSale.Client.Document; // CNPJ apenas números
			}
			else
			{
				dest.CPF = _currentSale?.Client?.Document ?? "99999999999"; // CPF apenas números
			}

			// Nome do destinatário (para homologação, usa texto padrão)
			dest.xNome = _currentFiscalConfiguration.Ambiente == AmbienteEnum.Homologacao
					? "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL"
					: _currentSale?.Client?.Name ?? "CONSUMIDOR NAO IDENTIFICADO";

			// Endereço do destinatário
			dest.enderDest = GetEnderecoDestinatario();

			// Configurações específicas por versão
			if (versao == VersaoServico.Versao200)
			{
				// Para versão 2.00
				if (!string.IsNullOrEmpty(_currentSale?.Client?.Ie))
					dest.IE = _currentSale.Client.Ie;
				else
					dest.IE = "ISENTO";

				return dest;
			}

			// Para versão 3.00 e superiores (NFCe/NFe)

			// Indicador de IE (1=Contribuinte, 2=Isento, 9=Não Contribuinte)
			if (_currentSale?.Client?.IndicadorIE != null)
			{
				switch (_currentSale.Client.IndicadorIE)
				{
					case "1": // Contribuinte
						dest.indIEDest = indIEDest.ContribuinteICMS;
						if (!string.IsNullOrEmpty(_currentSale.Client.Ie))
							dest.IE = _currentSale.Client.Ie;
						break;
					case "2": // Isento
						dest.indIEDest = indIEDest.Isento;
						dest.IE = "ISENTO";
						break;
					case "9": // Não Contribuinte
						dest.indIEDest = indIEDest.NaoContribuinte;
						break;
				}
			}
			else
			{
				// Default para NFCe: não contribuinte
				dest.indIEDest = indIEDest.NaoContribuinte;
			}

			// Email (opcional)
			if (!string.IsNullOrEmpty(_currentSale?.Client?.Email))
				dest.email = _currentSale.Client.Email;

			// Para NFCe em homologação, podemos usar email padrão
			else if (_currentFiscalConfiguration.Ambiente == AmbienteEnum.Homologacao)
				dest.email = "homologacao@sefaz.gov.br";

			return dest;
		}
		//protected virtual enderDest GetEnderecoDestinatario()
		//{
		//    var enderDest = new enderDest
		//    {
		//        xLgr = _currentSale?.Client?.Address?? "RUA ...",
		//        nro = _currentSale?.Client?.Numero?? "S/N",
		//        xBairro = _currentSale?.Client?.Bairro ?? "CENTRO",
		//        cMun =long.Parse( _currentSale?.Client?.InscricaoMunicipal) ,
		//        xMun = _currentSale?.Client?.Municipio?? "UBERLANDIA",
		//        UF = _currentSale?.Client?.Uf?? "MG",
		//        CEP = _currentSale?.Client?.ZipCode ??"49500000",
		//        cPais = 1058,
		//        xPais = "BRASIL"
		//    };
		//    return enderDest;
		//}
		protected virtual enderDest GetEnderecoDestinatario()
		{
			var endereco = new enderDest();
			var cliente = _currentSale?.Client;

			if (cliente != null)
			{
				endereco.xLgr = cliente.Address;
				endereco.nro = cliente.Numero;
				endereco.xCpl = cliente.Complemento;
				endereco.xBairro = cliente.Bairro;
				endereco.cMun = long.Parse(cliente.CodMunicipioIbge);
				endereco.xMun = cliente.Municipio; // Usa NameCity como fallback
				endereco.UF = cliente.Uf; // Usa NameState como fallback
				endereco.CEP = cliente.ZipCode;
				endereco.cPais = int.Parse(cliente.CodPais);
				endereco.xPais = cliente.Pais ?? "Brasil";

				if (!string.IsNullOrEmpty(cliente.CellPhone))
				{
					// Formata telefone (remover caracteres não numéricos)
					var telefone = new string(cliente.CellPhone.Where(char.IsDigit).ToArray());
					if (telefone.Length >= 10)
					{
						endereco.fone = long.Parse(telefone);
					}
				}
			}
			else
			{
				// Dados padrão para consumidor não identificado
				endereco.xLgr = "ENDERECO NAO INFORMADO";
				endereco.nro = "S/N";
				endereco.xBairro = "NAO INFORMADO";
				endereco.cMun = 9999999;
				endereco.xMun = "NAO INFORMADO";
				endereco.UF = "SP";
				endereco.CEP = "00000000";
				endereco.cPais = 1058;
				endereco.xPais = "Brasil";
			}

			return endereco;
		}
		protected virtual transp GetTransporte()
		{
			//var volumes = new List<vol> {GetVolume(), GetVolume()};

			var t = new transp
			{
				modFrete = ModalidadeFrete.mfSemFrete //NFCe: Não pode ter frete
																							//vol = volumes 
			};

			return t;
		}

		public async Task UpdateResultAsync(int id, bool sent, long? numero, string? responseJson, string? errorMessage)
		{
			var existing = await repository.GetByIdAsync(id);
			if (existing == null) throw new InvalidOperationException("Registro NFe não encontrado.");

			existing.Sent = sent;
			existing.Numero = numero ?? existing.Numero;
			existing.ResponseJson = responseJson;
			existing.ErrorMessage = errorMessage;
			existing.TryCount += 1;
			existing.UpdatedAt = DateTime.UtcNow;

			await repository.UpdateAsync(existing.Id, existing);
		}

		public async Task<NFeEmission?> GetByIdAsync(int id)
		{
			return await repository.GetByIdAsync(id);
		}

		public async Task<List<NFeEmission>> GetPendingAsync()
		{
			return await (repository as INFeRepository).GetPendingAsync();
		}

		public async Task<List<NFeEmission>> GetBySaleIdAsync(int saleId)
		{
			return await (repository as INFeRepository).GetBySaleIdAsync(saleId);
		}

		public async Task<long?> GetLastNumeroAsync(string serie, TipoDocumentoEnum tipoDocumento)
		{
			return await (repository as INFeRepository).GetLastNumeroAsync(serie, tipoDocumento);
		}
		public async Task<List<NFeEmission>> GetAll(int tenantid)
		{
			return await (repository as INFeRepository).GetAllAsync(tenantid);
		}
		public async Task<PagedResult<NFeEmission>> GetPaged(Filters filters)
		{
			return await (repository as INFeRepository).GetPaged(filters);
		}
		public async Task<byte[]> Danfe(int id)
		{
			NFeEmission nFeEmission = await repository.GetByIdAsync(id);
			//new nfeProc().CarregarDeXmlString(nFeEmission.XmlCompleto);//Funcoes.BuscarArquivoXml();
			try
			{
				nfeProc proc = null;
				NFe.Classes.NFe nfe = null;
				string arquivo = string.Empty;

				try
				{
					proc = new nfeProc().CarregarDeXmlString(nFeEmission.XmlCompleto);
					arquivo = proc.ObterXmlString();
				}
				catch (Exception)
				{
					nfe = new NFe.Classes.NFe().CarregarDeXmlString(nFeEmission.XmlCompleto);
					arquivo = nfe.ObterXmlString();
				}

				FiscalConfiguration fiscalConfiguration = await _fiscalConfigurationRepository.GetByCompany(nFeEmission.CompanyId);
				//if (fiscalConfiguration == null)
				//    return new ResponseGeneric { Success = false, Message = "Não encontrado as configurações para emissão de nota!" };

				DanfeNativoNfce impr = new DanfeNativoNfce(arquivo,
						VersaoQrCode.QrCodeVersao3,
					 null,
						fiscalConfiguration.Csc.Identificador,//_configuracoes.ConfiguracaoCsc.CIdToken,
						fiscalConfiguration.Csc.Valor,//",//_configuracoes.ConfiguracaoCsc.Csc,
						0 /*troco*//*, "Arial Black"*/);

				//SaveFileDialog fileDialog = new SaveFileDialog();

				//fileDialog.ShowDialog();

				//if (string.IsNullOrEmpty(fileDialog.FileName))
				//    throw new ArgumentException("Não foi selecionado nem uma pasta");

				return impr.PdfBytes();

				//impr.Imprimir(salvarArquivoPdfEm: fileDialog.FileName.Replace(".pdf", "") + ".pdf");
				//var bytes = impr.PdfBytes();
				//var base64 = Convert.ToBase64String(bytes);
			}
			catch (Exception ex)
			{
				return [];
			}
		}
		public async Task<byte[]> ObterXml(int id)
		{
			NFeEmission nfe = await repository.GetByIdAsync(id);
			byte[] xmlBytes = Encoding.UTF8.GetBytes(nfe.XmlCompleto);


			return xmlBytes;
		}
		public async Task<string> ObterNomeArquivoXml(int id)
		{
			NFeEmission nfe = await repository.GetByIdAsync(id);

			if (nfe == null)
				return $"nfe-{id}.xml";

			return $"nfe-{nfe.Numero}-{DateTime.Now:yyyyMMddHHmmss}.xml";
		}
		public async Task update(NFeEmissionDto attempt)
		{
			NFeEmission nFeEmission = await GetByIdAsync(attempt.Id);
			if (nFeEmission == null) return;
			nFeEmission.NaturezaOperacaoId = attempt.NaturezaOperacaoId;
			nFeEmission.SaleId = attempt.SaleId;
			nFeEmission.Numero = attempt.Numero;

			await repository.UpdateAsync(nFeEmission.Id, nFeEmission);
		}
		public async Task<ResponseGeneric> CancelarNfe(CancelarNotaRequest cancelarNota)
		{
			NFeEmission nFeEmission = await repository.GetByIdAsync(cancelarNota.Id);
			if (nFeEmission == null) return new ResponseGeneric { Success = false, Message = "NFe não encontrada." };
			try
			{
				//var idlote = Funcoes.InpuBox(this, titulo, "Identificador de controle do Lote de envio:");
				//if (string.IsNullOrEmpty(idlote)) throw new Exception("A Id do Lote deve ser informada!");

				//var sequenciaEvento = Funcoes.InpuBox(this, titulo, "Número sequencial do evento:");
				//if (string.IsNullOrEmpty(sequenciaEvento))
				//    throw new Exception("O número sequencial deve ser informado!");

				//var protocolo = Funcoes.InpuBox(this, titulo, "Protocolo de Autorização da NFe:");
				//if (string.IsNullOrEmpty(protocolo)) throw new Exception("O protocolo deve ser informado!");

				//var chave = Funcoes.InpuBox(this, titulo, "Chave da NFe:");
				//if (string.IsNullOrEmpty(chave)) throw new Exception("A Chave deve ser informada!");
				//if (chave.Length != 44) throw new Exception("Chave deve conter 44 caracteres!");

				//var justificativa = Funcoes.InpuBox(this, titulo, "Justificativa do cancelamento");
				//if (string.IsNullOrEmpty(justificativa)) throw new Exception("A justificativa deve ser informada!");
				FiscalConfiguration fiscalConfiguration = await _fiscalConfigurationRepository.GetByCompany(nFeEmission.CompanyId);
				NaturezaOperacao naturezaOperacao = await _naturezaOperacaoRepository.GetByIdAsync(nFeEmission.NaturezaOperacaoId);

				byte[] certbyte = await ObterCertificado(fiscalConfiguration.CertificadoDigital.Arquivo);
				//_currentFiscalConfiguration = fiscalConfiguration;
				//_currentNaturezaOperacao = naturezaOperacao;
				_configuracaoApp = criarConfiguracaoApp(fiscalConfiguration, naturezaOperacao, certbyte);
				var servicoNFe = new ServicosNFe(_configuracaoApp.CfgServico);
				var cpfcnpj = string.IsNullOrEmpty(_configuracaoApp.Emitente.CNPJ)
						? _configuracaoApp.Emitente.CPF
						: _configuracaoApp.Emitente.CNPJ;
				var retornoCancelamento = servicoNFe.RecepcaoEventoCancelamento(
						Convert.ToInt32(nFeEmission.Numero),
						Convert.ToInt16(1), nFeEmission.Protocolo, nFeEmission.ChaveAcesso, cancelarNota.Justificativa, cpfcnpj);

				int cstat = retornoCancelamento?.Retorno?.retEvento?.FirstOrDefault()?.infEvento?.cStat ?? 0;
				if (NfeSituacao.Cancelada(cstat))
				{

					nFeEmission.Sent = true;
					nFeEmission.MotivoCancelamento = cancelarNota.Justificativa;
					nFeEmission.UpdatedAt = DateTime.UtcNow;
					nFeEmission.StatusNfe = StatusNfe.cancelada;
					nFeEmission.XmlCompleto = retornoCancelamento.Retorno.ObterXmlString();
					await repository.UpdateAsync(nFeEmission.Id, nFeEmission);

					return new ResponseGeneric { Success = true };
				}
				else
				{
					return new ResponseGeneric
					{
						Success = false,
						Message = $"Erro ao cancelar NFe: {retornoCancelamento.Retorno.xMotivo}"
					};
				}


			}
			catch (Exception ex)
			{
				return new ResponseGeneric { Success = false, Message = $"Erro ao cancelar NFe: {ex.Message}" };
			}
		}
	}
	public interface INFeService
	{
		Task<ResponseGeneric> CreateAttemptAsync(NFeEmissionDto attempt);
		Task UpdateResultAsync(int id, bool sent, long? numero, string? responseJson, string? errorMessage);
		Task<NFeEmission?> GetByIdAsync(int id);
		Task<List<NFeEmission>> GetPendingAsync();
		Task<List<NFeEmission>> GetBySaleIdAsync(int saleId);
		Task<long?> GetLastNumeroAsync(string serie, TipoDocumentoEnum tipoDocumento);

		Task<List<NFeEmission>> GetAll(int tenantid);
		Task<PagedResult<NFeEmission>> GetPaged(Filters filters);
		Task<ResponseGeneric> Resend(int id);
		Task<byte[]> Danfe(int id);
		Task update(NFeEmissionDto attempt);
		Task<byte[]> ObterXml(int id);
		Task<string> ObterNomeArquivoXml(int id);
		Task<ResponseGeneric> CancelarNfe(CancelarNotaRequest cancelarNota);
		Task<ResponseGeneric> CreatedFromSale(NFeEmissionDto nfeDto);
	}
}