using DFe.Classes.Entidades;
using DFe.Classes.Flags;
using Microsoft.AspNetCore.Hosting;
using Model;
using Model.DTO;
using Model.DTO.NFe;
using Model.Enums;
using Model.Moves;
using Model.NFe;
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
using NFe.Classes.Informacoes.Detalhe.Tributacao.Municipal;
using NFe.Classes.Informacoes.Emitente;
using NFe.Classes.Informacoes.Identificacao;
using NFe.Classes.Informacoes.Identificacao.Tipos;
using NFe.Classes.Informacoes.Observacoes;
using NFe.Classes.Informacoes.Pagamento;
using NFe.Classes.Informacoes.Total;
using NFe.Classes.Informacoes.Transporte;
using NFe.Classes.Protocolo;
using NFe.Classes.Servicos.Tipos;
using NFe.Danfe.Base.NFe;
using NFe.Danfe.OpenFast.NFe;
using NFe.Danfe.QuestPdf.ImpressaoNfce;
using NFe.Danfe.QuestPdf.ImpressaoNfe;
using NFe.Servicos;
using NFe.Servicos.Retorno;
using NFe.Utils;
using NFe.Utils.Email;
using NFe.Utils.Evento;
using NFe.Utils.InformacoesSuplementares;
using NFe.Utils.NFe;
using Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
			private readonly ITributacaoResolverService _tributacaoResolver;
		private NFe.Classes.NFe _nfe;
		private ConfiguracaoApp _configuracaoApp;
		private FiscalConfiguration _currentFiscalConfiguration;
		private NaturezaOperacao _currentNaturezaOperacao;
		private Sale _currentSale;

		public NFeService(IGenericRepository<NFeEmission> repository,
					ISaleRepository saleRepository,
				IFiscalConfigurationRepository fiscalConfigurationRepository,
				INaturezaOperacaoRepository naturezaOperacaoRepository,
					IWebHostEnvironment webHostEnvironment,
					ITributacaoResolverService tributacaoResolver) : base(repository)
		{
			_saleRepository = saleRepository;
			_fiscalConfigurationRepository = fiscalConfigurationRepository;
			_naturezaOperacaoRepository = naturezaOperacaoRepository;
			_environment = webHostEnvironment;
				_tributacaoResolver = tributacaoResolver;
		}
		public async Task<ResponseGeneric> CreatedFromSale(NFeEmissionDto nfeDto)
		{
			List<NFeEmission> nFeEmissionList = await (repository as INFeRepository).GetBySaleIdAsync(nfeDto.SaleId);

			if (nFeEmissionList.Count > 0 )
			{
				NFeEmission nFeEmission = nFeEmissionList.FirstOrDefault(x => x.StatusNfe == StatusNfe.pendente);

				if (nFeEmission != null)
				{
					ResponseGeneric responseGeneric = await Resend(nFeEmission.Id, nfeDto.NaturezaOperacaoId);
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
				//pode ser nota cancelada
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

		// M�todo principal para reenvio
		public async Task<ResponseGeneric> Resend(int id, int? nop)
		{
			NFeEmission nFeEmission = await (repository as INFeRepository).GetByIdAsync(id);
		
			if (nFeEmission == null)
				return new ResponseGeneric { Success = false, Message = "N�o foi encontrado a nota!" };
			if (nop != null)
				nFeEmission.NaturezaOperacaoId = nop ?? nFeEmission.NaturezaOperacaoId;
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

		// M�todo principal para cria��o
		public async Task<ResponseGeneric> CreateAttemptAsync(NFeEmissionDto attempt)
		{
			var (validationResult, fiscalConfig, sale, naturezaOperacao) =
					await ValidateAndGetDependencies(attempt.CompanyId, attempt.SaleId, attempt.NaturezaOperacaoId);

			if (!validationResult.Success)
				return validationResult;

			NFeEmission ultimaNota = await (repository as INFeRepository).GetByCompany(attempt.CompanyId);
			int proximoNumeroNfe = CalculateNextNumber(ultimaNota, fiscalConfig);

			var respEmissao = await TransmitirNfe(proximoNumeroNfe, fiscalConfig, sale, naturezaOperacao);

			var entity = CreateNFeEmission(attempt, respEmissao, fiscalConfig, proximoNumeroNfe, naturezaOperacao);

			await repository.CreateAsync(entity);
			return new ResponseGeneric { Success = true, Data = entity };
		}

		// M�todo privado para valida��es comuns
		private async Task<(ResponseGeneric ValidationResult, FiscalConfiguration FiscalConfig, Sale Sale, NaturezaOperacao NaturezaOperacao)>
				ValidateAndGetDependencies(int companyId, int saleId, int naturezaOperacaoId)
		{
			// Configura��o da empresa
			FiscalConfiguration fiscalConfiguration = await _fiscalConfigurationRepository.GetByCompany(companyId);
			if (fiscalConfiguration == null)
				return (new ResponseGeneric { Success = false, Message = "N�o encontrado as configurações para emiss�o de nota!" }, null, null, null);

			// Verifica se existe a venda
			Sale sale = await _saleRepository.GetSaleByCompany(saleId, companyId);
			if (sale == null)
				return (new ResponseGeneric { Success = false, Message = "Venda n�o encontrada para a empresa." }, null, null, null);

			if (sale.Financials == null || sale.Financials.Count() == 0)
				return (new ResponseGeneric { Success = false, Message = "Venda n�o possui financeiro!" }, null, null, null);

			// Natureza da opera��o
			NaturezaOperacao naturezaOperacao = await _naturezaOperacaoRepository.GetByIdAsync(naturezaOperacaoId);
			if (naturezaOperacao == null)
				return (new ResponseGeneric { Success = false, Message = "Natureza de operação n�o encontrada." }, null, null, null);

			return (new ResponseGeneric { Success = true }, fiscalConfiguration, sale, naturezaOperacao);
		}

		// M�todo privado para atualizar entidade existente
		private void UpdateNFeEmission(NFeEmission nFeEmission, object respEmissao, FiscalConfiguration fiscalConfig)
		{
			nFeEmission.Sent = true;
			nFeEmission.Serie = fiscalConfig.NumeracaoDocumentos.Nfce.Serie;
			nFeEmission.TryCount += 1;
			nFeEmission.UpdatedAt = DateTime.Now;

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

		// M�todo privado para criar nova entidade
		private NFeEmission CreateNFeEmission(NFeEmissionDto attempt, object respEmissao, FiscalConfiguration fiscalConfig, int numero, NaturezaOperacao naturezaOperacao)
		{
			var entity = new NFeEmission
			{
				NaturezaOperacaoId = attempt.NaturezaOperacaoId,
				SaleId = attempt.SaleId,
				TipoDocumento = naturezaOperacao.TipoDocumento == TipoDocumentoEnum.NFE ? TipoDocumentoEnum.NFE : TipoDocumentoEnum.NFCE,
				Serie = fiscalConfig.NumeracaoDocumentos.Nfce.Serie,
				Numero = numero,
				CreatedAt = DateTime.Now,
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

		// M�todo para calcular pr�ximo n�mero
		private int CalculateNextNumber(NFeEmission ultimaNota, FiscalConfiguration fiscalConfig)
		{
			if (ultimaNota == null)
				return Convert.ToInt32(fiscalConfig.NumeracaoDocumentos.Nfce.NumeroInicial);

			return Convert.ToInt32(ultimaNota.Numero + 1);
		}

		private X509Certificate2 ValidarCertificado(byte[] certBytes, string senha)
		{
			var cert = new X509Certificate2(
					certBytes,
					senha,
					X509KeyStorageFlags.MachineKeySet |
					X509KeyStorageFlags.PersistKeySet |
					X509KeyStorageFlags.Exportable
			);

			Console.WriteLine("===== VALIDA��O CERTIFICADO =====");
			Console.WriteLine($"Subject: {cert.Subject}");
			Console.WriteLine($"Issuer: {cert.Issuer}");
			Console.WriteLine($"Valido de: {cert.NotBefore}");
			Console.WriteLine($"Valido at�: {cert.NotAfter}");
			Console.WriteLine($"HasPrivateKey: {cert.HasPrivateKey}");
			Console.WriteLine($"Thumbprint: {cert.Thumbprint}");

			if (!cert.HasPrivateKey)
				throw new Exception("Certificado sem chave privada (HasPrivateKey=false)");

			if (DateTime.Now < cert.NotBefore || DateTime.Now > cert.NotAfter)
				throw new Exception("Certificado expirado ou ainda n�o v�lido");

			return cert;
		}
		private async Task<dynamic> TransmitirNfe(int numero, FiscalConfiguration fiscalConfiguration, Sale sale, NaturezaOperacao naturezaOperacao)
		{
			try
			{
				_currentFiscalConfiguration = fiscalConfiguration;
				_currentNaturezaOperacao = naturezaOperacao;
				_currentSale = sale;
				ValidarTipoDocumentoPorVenda();
				_configuracaoApp = criarConfiguracaoApp(fiscalConfiguration, naturezaOperacao);
				ModeloDocumento modeloDocumento = _currentNaturezaOperacao.TipoDocumento == TipoDocumentoEnum.NFCE ? ModeloDocumento.NFCe : ModeloDocumento.NFe;
					_nfe = await ObterNfeValidadaAsync(VersaoServico.Versao400, modeloDocumento,
						numero, new ConfiguracaoCsc
						{
							CIdToken = fiscalConfiguration.Csc.Identificador,
							Csc = fiscalConfiguration.Csc.Valor
						});
				//_nfe.infNFeSupl.ObterUrlQrCode3();
				var xml = _nfe.ObterXmlString();
				ServicePointManager.Expect100Continue = false;
				ServicePointManager.SecurityProtocol =
						SecurityProtocolType.Tls12 |
						SecurityProtocolType.Tls11 |
						SecurityProtocolType.Tls;

				var servicoNFe = new ServicosNFe(_configuracaoApp.CfgServico);

					// ============================================================
					// DIAGNOSTICO: Teste de copia do certificado
					// Simula o que as classes NFe.Wsdl.Standard fazem:
					//   new X509Certificate2(certificado)
					// ============================================================
					try
					{
						var certOriginal = DFe.Utils.Assinatura.CertificadoDigital.ObterCertificado(_configuracaoApp.CfgServico.Certificado);
						Console.WriteLine("================================================");
						Console.WriteLine("DIAGNOSTICO: COMPARACAO CERTIFICADO ORIGINAL vs COPIA");
						Console.WriteLine("================================================");

						Console.WriteLine("--- ORIGINAL ---");
						Console.WriteLine("  HasPrivateKey: " + certOriginal.HasPrivateKey);
						Console.WriteLine("  Thumbprint:   " + certOriginal.Thumbprint);
						Console.WriteLine("  Handle:       " + certOriginal.Handle);
						Console.WriteLine("  Subject:      " + certOriginal.Subject);
						Console.WriteLine("  HashCode:     " + certOriginal.GetHashCode());
						try
						{
							var rsaOrig = certOriginal.GetRSAPrivateKey();
							Console.WriteLine("  RSA Key:      " + (rsaOrig != null ? rsaOrig.GetType().FullName + " (" + rsaOrig.KeySize + " bits)" : "NULL !"));
							if (rsaOrig != null)
							{
								var testData = System.Text.Encoding.UTF8.GetBytes("TESTE_ASSINATURA_DIAGNOSTICO");
								var sig = rsaOrig.SignData(testData, System.Security.Cryptography.HashAlgorithmName.SHA256, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
								Console.WriteLine("  SignData:     OK (" + sig.Length + " bytes)");
							}
						}
						catch (Exception rsaEx)
						{
							Console.WriteLine("  RSA Key:      ERRO: " + rsaEx.Message);
						}

						// Simula o que as classes Standard fazem: new X509Certificate2(certificado)
						X509Certificate2 certCopia = null;
						try
						{
							certCopia = new X509Certificate2(certOriginal);
							Console.WriteLine("");
							Console.WriteLine("--- COPIA (new X509Certificate2(original)) ---");
							Console.WriteLine("  HasPrivateKey: " + certCopia.HasPrivateKey);
							Console.WriteLine("  Thumbprint:   " + certCopia.Thumbprint);
							Console.WriteLine("  Handle:       " + certCopia.Handle);
							Console.WriteLine("  Subject:      " + certCopia.Subject);
							Console.WriteLine("  HashCode:     " + certCopia.GetHashCode());
							try
							{
								var rsaCopia = certCopia.GetRSAPrivateKey();
								Console.WriteLine("  RSA Key:      " + (rsaCopia != null ? rsaCopia.GetType().FullName + " (" + rsaCopia.KeySize + " bits)" : "NULL !"));
								if (rsaCopia != null)
								{
									var testData = System.Text.Encoding.UTF8.GetBytes("TESTE_ASSINATURA_DIAGNOSTICO");
									var sig = rsaCopia.SignData(testData, System.Security.Cryptography.HashAlgorithmName.SHA256, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
									Console.WriteLine("  SignData:     OK (" + sig.Length + " bytes)");
								}
							}
							catch (Exception rsaEx)
							{
								Console.WriteLine("  RSA Key:      ERRO: " + rsaEx.Message);
							}

							// Teste de export Pkcs12
							try
							{
								var exported = certCopia.Export(X509ContentType.Pkcs12);
								Console.WriteLine("  Export Pkcs12: OK (" + exported.Length + " bytes)");
							}
							catch (Exception expEx)
							{
								Console.WriteLine("  Export Pkcs12: FALHOU: " + expEx.Message);
							}
						}
						catch (Exception copiaEx)
						{
							Console.WriteLine("  ERRO ao criar copia: " + copiaEx.Message);
						}

						// Conclusao
						Console.WriteLine("");
						Console.WriteLine("--- CONCLUSAO ---");
						if (certCopia != null)
						{
							bool mesmaInstancia = object.ReferenceEquals(certOriginal, certCopia);
							bool mesmoHandle = certOriginal.Handle == certCopia.Handle;
							bool mesmoThumb = certOriginal.Thumbprint == certCopia.Thumbprint;
							bool copiaTemChave = certCopia.HasPrivateKey;
							bool copiaTemRSA = certCopia.GetRSAPrivateKey() != null;

							Console.WriteLine("  Mesma instancia (ReferenceEquals): " + mesmaInstancia);
							Console.WriteLine("  Mesmo Handle:                       " + mesmoHandle);
							Console.WriteLine("  Mesmo Thumbprint:                   " + mesmoThumb);
							Console.WriteLine("  Copia HasPrivateKey:                " + copiaTemChave);
							Console.WriteLine("  Copia GetRSAPrivateKey() != null:   " + copiaTemRSA);

							if (!copiaTemChave || !copiaTemRSA)
								Console.WriteLine("  ALERTA: A COPIA PERDEU A CHAVE PRIVADA!");
							else
								Console.WriteLine("  Copia preservou a chave privada");
						}
						Console.WriteLine("================================================");
					}
					catch (Exception diagEx)
					{
						Console.WriteLine("Erro no diagnostico: " + diagEx.Message);
					}
					// ============================================================
				Console.WriteLine("=== INICIANDO TRANSMISS�O NFCe ===");
				Console.WriteLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
				Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("RENDER")}");
				var retornoEnvio = servicoNFe.NFeAutorizacao(int.Parse(fiscalConfiguration.NumeracaoDocumentos.Nfce.Serie), 
					IndicadorSincronizacao.Sincrono, new List<NFe.Classes.NFe> { _nfe }, false/*Envia a mensagem compactada para a SEFAZ*/);
				Console.WriteLine($"retorno: {retornoEnvio.Retorno.xMotivo}");

				retornoEnvio.Xml = xml;

				return retornoEnvio;
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
			finally
			{
				_currentFiscalConfiguration = null;
				_currentNaturezaOperacao = null;
				_currentSale = null;
			}
		}
		private void ValidarTipoDocumentoPorVenda()
		{
			var tipoDocumento = _currentNaturezaOperacao.TipoDocumento;

			if (tipoDocumento == TipoDocumentoEnum.NFE)
			{
				// NFe: Destinatário OBRIGATÓRIO
				if (_currentSale?.Client == null)
				{
					throw new Exception(
							"A venda não possui cliente cadastrado. " +
							"NFe exige destinatário com CPF/CNPJ. " +
							"Considere usar NFCe se for consumidor final não identificado."
					);
				}

				// Validações adicionais para NFe
				var cliente = _currentSale.Client;

				// CPF ou CNPJ deve ser válido
				if (string.IsNullOrEmpty(cliente.Document) || cliente.Document.Length < 11)
				{
					throw new Exception(
							$"Documento do destinatário inválido: {cliente.Document}. " +
							"NFe exige CPF ou CNPJ válido."
					);
				}

				// Endereço deve estar completo
				if (string.IsNullOrEmpty(cliente.Address) ||
						string.IsNullOrEmpty(cliente.Municipio) ||
						string.IsNullOrEmpty(cliente.Uf))
				{
					throw new Exception(
							"Endereço do destinatário incompleto. " +
							"Logradouro, município e UF são obrigatórios para NFe."
					);
				}

				// IE é obrigatória para alguns estados (quando contribuinte)
				if (cliente.IndicadorIE == "1" && string.IsNullOrEmpty(cliente.Ie))
				{
					throw new Exception(
							"Inscrição Estadual obrigatória para contribuintes do ICMS."
					);
				}
			}
			else if (tipoDocumento == TipoDocumentoEnum.NFCE)
			{
				// NFCe: Aceita destinatário nulo (consumidor não identificado)
				if (_currentSale?.Client != null)
				{
					// Se tiver cliente, valida que o documento é de pessoa física
					if (_currentSale.Client.TipoPessoa == "J")
					{
						// ⚠️ ATENÇÃO: NFCe normalmente só aceita CPF (pessoa física)
						// Alguns estados aceitam CNPJ para NFCe, verificar legislação local
						var cliente = _currentSale.Client;
						if (cliente.Document.Length == 14 && !string.IsNullOrEmpty(cliente.Ie))
						{
							// Log de aviso, mas não bloqueia
							Console.WriteLine($"AVISO: NFCe com CNPJ ({cliente.Document}). " +
															 "Verifique se a SEFAZ do seu estado aceita.");
						}
					}
				}
			}
		}
		/// <summary>
		/// Converte o CRT (Codigo de Regime Tributario) do cadastro para o enum do DFe.
		/// 1 = Simples Nacional, 2 = Simples Nacional - MEI, 3 = Regime Normal
		/// </summary>
		private static CRT ObterCRT(string? crt)
		{
			return crt switch
			{
				"1" => CRT.SimplesNacional,
				"2" => CRT.SimplesNacionalMei,
				_ => CRT.RegimeNormal
			};
		}

		public static string LimparString(string texto, bool manterEspacos = false)
		{
			if (string.IsNullOrEmpty(texto))
				return texto;

			if (manterEspacos)
			{
				// Mant�m letras, n�meros e espa�os
				return Regex.Replace(texto, @"[^a-zA-Z0-9\s]", "");
			}
			else
			{
				// Mant�m apenas letras e n�meros
				return Regex.Replace(texto, @"[^a-zA-Z0-9]", "");
			}
		}
		private string OnSucessoSync(RetornoBasico e)
		{

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
		
		private ConfiguracaoApp criarConfiguracaoApp(FiscalConfiguration fiscalConfiguration, NaturezaOperacao naturezaOperacao)
		{
			try
			{

				string nomeArquivo = Path.GetFileName(fiscalConfiguration.CertificadoDigital.Arquivo);

				string caminhoCertificado =
						Environment.GetEnvironmentVariable("RENDER") == "true"
								? Path.Combine("/app/wwwroot/certs", nomeArquivo)
								: Path.Combine(_environment.WebRootPath, "certs", nomeArquivo);

				var Certificado = new DFe.Utils.ConfiguracaoCertificado
				{
					TipoCertificado = DFe.Utils.TipoCertificado.A1Arquivo,
					Arquivo = caminhoCertificado,
					Senha = fiscalConfiguration.CertificadoDigital.Senha,
					ManterDadosEmCache = false,
					KeyStorageFlags =
								X509KeyStorageFlags.UserKeySet |
								X509KeyStorageFlags.PersistKeySet |
								X509KeyStorageFlags.Exportable
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
						ValidarCertificadoDoServidor = false,
						Certificado = Certificado
					},
					Emitente = new emit
					{
						CNPJ = LimparString(fiscalConfiguration.Emitente.Cnpj),
						IE = LimparString(fiscalConfiguration.Emitente.InscricaoEstadual),
						xNome = fiscalConfiguration.Emitente.RazaoSocial,
						xFant = fiscalConfiguration.Emitente.Fantasia,
						CRT = ObterCRT(fiscalConfiguration.Emitente.RegimeTributario.Crt),

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
					EnviarTributacaoIbsCbs = true,
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
			private async Task<NFe.Classes.NFe> ObterNfeValidadaAsync(VersaoServico versaoServico, ModeloDocumento modelo, int numero,
				ConfiguracaoCsc configuracaoCsc)
		{
				var nfe = await GetNfAsync(numero, modelo, versaoServico);

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
			protected virtual async Task<NFe.Classes.NFe> GetNfAsync(int numero, ModeloDocumento modelo, VersaoServico versao)
			{
				var nf = new NFe.Classes.NFe { infNFe = await GetInfAsync(numero, modelo, versao) };
			return nf;
		}
		protected virtual async Task<infNFe> GetInfAsync(int numero, ModeloDocumento modelo, VersaoServico versao)
		{
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
					TributacaoResolvida? ultimaTributacao = null;
				foreach (var i in _currentSale.SaleItems)
				{
					index++;

						// Resolve tributacao pela matriz (NaturezaOperacao + SituacaoTributaria + Destino)
						var empresaUf = _currentFiscalConfiguration?.Emitente?.EmitenteEndereco?.Uf ?? "";
						var clienteUf = _currentSale?.Client?.Uf ?? "";
						var clienteCodPais = _currentSale?.Client?.CodPais ?? "1058";

						var tributacaoResolvida = await _tributacaoResolver.ResolverTributacaoAsync(
							i.IdProduct,
							_currentNaturezaOperacao.Id,
							empresaUf,
							clienteUf,
							clienteCodPais);

					infNFe.det.Add(GetDetalhe(index, i, infNFe.emit.CRT, modelo, tributacaoResolvida));
						ultimaTributacao = tributacaoResolvida;
				}

				infNFe.total = GetTotal(versao, infNFe.det, modelo, ultimaTributacao ?? new TributacaoResolvida());

				if (infNFe.ide.mod == ModeloDocumento.NFe & (versao == VersaoServico.Versao310 || versao == VersaoServico.Versao400))
					infNFe.cobr = GetCobranca(infNFe.total.ICMSTot);
				if (infNFe.ide.mod == ModeloDocumento.NFCe || (infNFe.ide.mod == ModeloDocumento.NFe & versao == VersaoServico.Versao400))
					infNFe.pag = GetPagamento(infNFe.total.ICMSTot, versao);

				if (infNFe.ide.mod == ModeloDocumento.NFCe & versao != VersaoServico.Versao400)
					infNFe.infAdic = new infAdic() { infCpl = "" };

				return infNFe;
			}
		}
		
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

			// Extrair todos os detalhes de pagamento com seus valores individuais
			var detalhesPagamento = new List<PagamentoDetalhe>();

			foreach (var financial in pagamentos)
			{
				if (financial.FinancialPaymentMethods != null && financial.FinancialPaymentMethods.Any())
				{
					foreach (var paymentMethod in financial.FinancialPaymentMethods)
					{
						detalhesPagamento.Add(new PagamentoDetalhe
						{
							PaymentMethod = paymentMethod.PaymentMethod,
							Value = paymentMethod.Amount, // Assumindo que existe um campo Value
							FinancialId = financial.Id // Para refer�ncia se precisar
						});
					}
				}
				else
				{
					// Fallback para o caso de n�o ter FinancialPaymentMethods
					//detalhesPagamento.Add(new PagamentoDetalhe
					//{
					//	PaymentMethod = financial.FinancialPaymentMethods.,
					//	Value = financial.Value,
					//	FinancialId = financial.Id
					//});
				}
			}

			if (!detalhesPagamento.Any())
				return null;

			decimal totalPagamentos = detalhesPagamento.Sum(d => d.Value);
			decimal totalNF = icmsTot.vNF;

			// Valida��o: o total dos pagamentos deve ser igual ao total da NF
			if (Math.Abs(totalPagamentos - totalNF) > 0.01m)
			{
				// Ajusta o �ltimo pagamento para igualar
				var ultimoPagamento = detalhesPagamento.Last();
				ultimoPagamento.Value = totalNF - (totalPagamentos - ultimoPagamento.Value);
			}

			// Vers�o 3.10 ou inferior
			if (versao != VersaoServico.Versao400)
			{
				return detalhesPagamento.Select(d => new pag
				{
					tPag = ConverterPorNome(d.PaymentMethod.Name),
					vPag = Math.Round(d.Value, 2),
				}).ToList();
			}

			// Vers�o 4.00
			var pagamentosV4 = new pag
			{
				detPag = detalhesPagamento.Select(d =>
				{
					var formaPagamento = ConverterPorNome(d.PaymentMethod.Name);
					var detPagObj = new detPag
					{
						tPag = formaPagamento,
						vPag = Math.Round(d.Value, 2)
					};

					// Adicionar dados do cart�o se for cart�o de cr�dito OU cart�o de d�bito
					if (formaPagamento == FormaPagamento.fpCartaoCredito ||
							formaPagamento == FormaPagamento.fpCartaoDebito)
					{
						detPagObj.card = new card
						{
							tpIntegra = TipoIntegracaoPagamento.TipNaoIntegrado,
							cAut = "NAOINTEGRADO",
							CNPJ = _configuracaoApp.Emitente.CNPJ,
							tBand = BandeiraCartao.bcOutros
						};
					}

					// Adicionar descri��o se for "Outros" (99)
					if (formaPagamento == FormaPagamento.fpOutro)
					{
						detPagObj.xPag = d.PaymentMethod?.Name ?? "Outros";
					}

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

			if (nomeUpper.Contains("CR�DITO") || nomeUpper.Contains("CREDITO"))
				return FormaPagamento.fpCartaoCredito;

			if (nomeUpper.Contains("D�BITO") || nomeUpper.Contains("DEBITO"))
				return FormaPagamento.fpCartaoDebito;
			if (nomeUpper.Contains("BOLETO") || nomeUpper.Contains("DUPLICATA"))
				return FormaPagamento.fpBoletoBancario;

			return FormaPagamento.fpOutro;
		}
		

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
			// SE FOR PAGAMENTO � VISTA (apenas 1 parcela na mesma data ou sem prazo)
			// N�O deve informar cobran�a
			if (financials.Count == 1 && financials.First().DueDate <= DateTime.Now.AddDays(1))
			{
				return null; // Pagamento � vista - n�o envia <cobr>
			}

			decimal totalParcelas = financials.Sum(f => f.Value);
			decimal totalNF = icmsTot.vNF;

			// Valida��o: a soma das parcelas deve ser igual ao total da NF
			// (considerando uma pequena margem de erro por arredondamento)
			if (Math.Abs(totalParcelas - totalNF) > 0.01m)
			{
				// Log de aviso ou ajuste autom�tico
				// Pode-se ajustar a �ltima parcela para igualar
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
		
		private bool ValidarGTIN(string gtin)
		{
			if (string.IsNullOrEmpty(gtin))
				return false;

			// Remove espa�os e caracteres n�o num�ricos
			gtin = new string(gtin.Where(char.IsDigit).ToArray());

			// Verifica se tem 8, 12, 13 ou 14 d�gitos
			if (gtin.Length != 8 && gtin.Length != 12 && gtin.Length != 13 && gtin.Length != 14)
				return false;

			// Valida��o b�sica - n�o pode come�ar com zero
			if (gtin.StartsWith("0"))
				return false;

			// Valida d�gito verificador (opcional, mas recomendado)
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
				return true; // Para outros tamanhos, n�o valida
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
		protected virtual prod GetProduto(SaleItems i, string cfop)
		{
			string gtin = i.Product.Code ?? "";
			bool isGtinValido = ValidarGTIN(gtin);
			var p = new prod
			{
				cProd = i.Product.Id.ToString().PadLeft(5, '0'),
				cEAN = isGtinValido ? gtin : "SEM GTIN",
				xProd = _currentFiscalConfiguration.Ambiente == AmbienteEnum.Homologacao
						? "NOTA FISCAL EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL"  // Texto padr�o para produto
						: i.Product.Name,
				NCM = i.Product.Ncm,
				CFOP = int.Parse(cfop),
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
				CEST = i.Product.Cest ?? "0000000"

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
		
			protected virtual det GetDetalhe(int index, SaleItems i, CRT crt, ModeloDocumento modelo, TributacaoResolvida tributacao)
		{
			var produto = GetProduto(i, tributacao.Cfop);
			var config = tributacao.Configuracao ?? new ConfiguracaoTributaria();

			// Usa CalculadorImpostos para centralizar toda a lógica de cálculo
			var calculador = new CalculadorImpostos(config, i, crt);

			// Determina o tipo de ICMS (regime normal vs Simples Nacional)
			var tipoICMS = calculador.IsSimplesNacional()
				? InformarCSOSN(config, i)
				: InformarICMS(config, i);

			var det = new det
			{
				nItem = index,
				prod = produto,
				imposto = new imposto
				{
					vTotTrib = calculador.CalcularTotalTributos(),
					ICMS = new ICMS { TipoICMS = tipoICMS },
					COFINS = calculador.CalcularCOFINS(),
					PIS = calculador.CalcularPIS(),
					//IBSCBS = calculador.CalcularIBSCBS()
				}
			};

			// IPI apenas para NFe (NFCe não aceita grupo IPI)
			det.imposto.IPI = calculador.CalcularIPI(modelo);

			// IS apenas para NFe
			if (modelo == ModeloDocumento.NFe)
			{
				det.imposto.IS = calculador.MontarIS();
				det.imposto.ISSQN = calculador.CalcularISSQN();
			}

			return det;
		}

		// M�todos auxiliares
			private decimal CalcularTotalTributos(SaleItems item, ConfiguracaoTributaria config)
		{
			decimal valorTotalItem = item.Value * item.Amount;
			decimal totalTributos = 0;

			// Adicionar COFINS se aplic�vel
			if (config.AplicarCOFINS && config.AliquotaCOFINS > 0)
			{
				totalTributos += valorTotalItem * config.AliquotaCOFINS / 100;
			}

			// Adicionar PIS se aplic�vel
			if (config.AplicarPIS && config.AliquotaPIS > 0)
			{
				totalTributos += valorTotalItem * config.AliquotaPIS / 100;
			}

			// Adicionar IPI se aplic�vel (apenas para NFe)
			if (config.AplicarIPI && config.AliquotaIPI > 0)
			{
				totalTributos += valorTotalItem * config.AliquotaIPI / 100;
			}

			// Adicionar ICMS aproximado se dispon�vel
			var icms = CalcularICMSAproximado(item, config);
			if (icms > 0)
			{
				totalTributos += icms;
			}

			return Math.Round(totalTributos, 2);
		}

			private decimal CalcularValorCOFINS(decimal valorTotalItem, decimal aliquota, ConfiguracaoTributaria config)
		{
			if (!config.AplicarCOFINS || aliquota <= 0)
				return 0;

			return Math.Round(valorTotalItem * aliquota / 100, 2);
		}

			private decimal CalcularValorPIS(decimal valorTotalItem, decimal aliquota, ConfiguracaoTributaria config)
		{
			if (!config.AplicarPIS || aliquota <= 0)
				return 0;

			return Math.Round(valorTotalItem * aliquota / 100, 2);
		}

			private decimal CalcularValorIPI(decimal valorTotalItem, decimal aliquota, ConfiguracaoTributaria config)
		{
			if (!config.AplicarIPI || aliquota <= 0)
				return 0;

			return Math.Round(valorTotalItem * aliquota / 100, 2);
		}

			private decimal CalcularICMSAproximado(SaleItems item, ConfiguracaoTributaria config)
		{
			// Implementar c�lculo do ICMS aproximado baseado na configura��o
			// Este � um valor aproximado para o vTotTrib
			decimal valorTotalItem = item.Value * item.Amount;
			decimal aliquotaICMS = 18; // Al�quota padr�o, ajuste conforme necessidade

			// Verificar se � Simples Nacional
			if (_configuracaoApp.Emitente.CRT == CRT.SimplesNacional)
			{
				// Para Simples Nacional, usar al�quota aproximada do produto
				aliquotaICMS = 7; // Exemplo: 7% para Simples Nacional
			}

			return Math.Round(valorTotalItem * aliquotaICMS / 100, 2);
		}
		/// <summary>
		/// Retorna o ICMSBasico adequado para o CST configurado, com valores calculados dinamicamente.
		/// </summary>
		protected virtual ICMSBasico InformarICMS(ConfiguracaoTributaria config, SaleItems item)
		{
			var cst = CalculadorImpostos.ObterCSTICMS(config.CstICMS);
			var calculador = new CalculadorImpostos(config, item, _configuracaoApp?.Emitente?.CRT ?? CRT.SimplesNacional);
			var (baseCalculo, valorICMS, aliquota) = calculador.CalcularICMSNormal();
			var valorTotalItem = calculador.ValorTotalItem;

			switch (cst)
			{
				case Csticms.Cst00:
					return new ICMS00
					{
						orig = OrigemMercadoria.OmNacional,
						CST = Csticms.Cst00,
						modBC = DeterminacaoBaseIcms.DbiValorOperacao,
						vBC = baseCalculo,
						pICMS = aliquota,
						vICMS = valorICMS
					};

				case Csticms.Cst10:
					return new ICMS10
					{
						orig = OrigemMercadoria.OmNacional,
						CST = Csticms.Cst10,
						modBC = DeterminacaoBaseIcms.DbiValorOperacao,
						vBC = baseCalculo,
						pICMS = aliquota,
						vICMS = valorICMS,
						modBCST = DeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
						vBCST = valorTotalItem,
						pICMSST = 0,
						vICMSST = 0
					};

				case Csticms.Cst20:
					return new ICMS20
					{
						orig = OrigemMercadoria.OmNacional,
						CST = Csticms.Cst20,
						modBC = DeterminacaoBaseIcms.DbiValorOperacao,
						pRedBC = config.ReduzirBaseICMS ? aliquota : 0,
						vBC = baseCalculo,
						pICMS = aliquota,
						vICMS = valorICMS
					};

				case Csticms.Cst30:
					return new ICMS30
					{
						orig = OrigemMercadoria.OmNacional,
						CST = Csticms.Cst30,
						modBCST = DeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
						vBCST = 0,
						pICMSST = 0,
						vICMSST = 0
					};

				case Csticms.Cst40:
					return new ICMS40
					{
						orig = OrigemMercadoria.OmNacional,
						CST = Csticms.Cst40
					};

				case Csticms.Cst41:
					return new ICMS40
					{
						orig = OrigemMercadoria.OmNacional,
						CST = Csticms.Cst41
					};

				case Csticms.Cst50:
					return new ICMS40
					{
						orig = OrigemMercadoria.OmNacional,
						CST = Csticms.Cst50
					};

				case Csticms.Cst51:
					return new ICMS51
					{
						orig = OrigemMercadoria.OmNacional,
						CST = Csticms.Cst51,
						modBC = DeterminacaoBaseIcms.DbiValorOperacao,
						vBC = valorTotalItem,
						pICMS = aliquota,
						vICMS = valorICMS
					};

				case Csticms.Cst60:
					return new ICMS60
					{
						orig = OrigemMercadoria.OmNacional,
						CST = Csticms.Cst60,
						vBCSTRet = 0,
						vICMSSTRet = 0
					};

				case Csticms.Cst70:
					return new ICMS70
					{
						orig = OrigemMercadoria.OmNacional,
						CST = Csticms.Cst70,
						modBC = DeterminacaoBaseIcms.DbiValorOperacao,
						pRedBC = config.ReduzirBaseICMS ? aliquota : 0,
						vBC = baseCalculo,
						pICMS = aliquota,
						vICMS = valorICMS,
						modBCST = DeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
						vBCST = 0,
						pICMSST = 0,
						vICMSST = 0
					};

				case Csticms.Cst90:
					return new ICMS90
					{
						orig = OrigemMercadoria.OmNacional,
						CST = Csticms.Cst90,
						modBC = DeterminacaoBaseIcms.DbiValorOperacao,
						vBC = baseCalculo,
						pICMS = aliquota,
						vICMS = valorICMS,
						modBCST = DeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
						vBCST = 0,
						pICMSST = 0,
						vICMSST = 0
					};

				default:
					// Fallback: CST 00 – tributação integral
					return new ICMS00
					{
						orig = OrigemMercadoria.OmNacional,
						CST = Csticms.Cst00,
						modBC = DeterminacaoBaseIcms.DbiValorOperacao,
						vBC = baseCalculo,
						pICMS = aliquota,
						vICMS = valorICMS
					};
			}
		}
		/// <summary>
		/// Retorna o ICMSBasico adequado para o CSOSN configurado (Simples Nacional).
		/// </summary>
		protected virtual ICMSBasico InformarCSOSN(ConfiguracaoTributaria config, SaleItems item)
		{
			// Prioridade: CsosnICMS (campo especifico) > CstICMS (compatibilidade)
			var csosnCode = !string.IsNullOrWhiteSpace(config.CsosnICMS) ? config.CsosnICMS : config.CstICMS;
			var csosn = CalculadorImpostos.ObterCSOSN(csosnCode);

			switch (csosn)
			{
				case Csosnicms.Csosn101:
					return new ICMSSN101
					{
						orig = OrigemMercadoria.OmNacional,
						CSOSN = Csosnicms.Csosn101,
						pCredSN = config.AliquotaICMS > 0 ? config.AliquotaICMS : 0m,
						vCredICMSSN = config.AliquotaICMS > 0
							? Math.Round(item.Value * item.Amount * config.AliquotaICMS / 100, 2)
							: 0m
					};

				case Csosnicms.Csosn102:
					return new ICMSSN102
					{
						orig = OrigemMercadoria.OmNacional,
						CSOSN = Csosnicms.Csosn102
					};

				case Csosnicms.Csosn103:
					return new ICMSSN102
					{
						orig = OrigemMercadoria.OmNacional,
						CSOSN = Csosnicms.Csosn103
					};

				case Csosnicms.Csosn201:
					return new ICMSSN201
					{
						orig = OrigemMercadoria.OmNacional,
						CSOSN = Csosnicms.Csosn201,
						modBCST = DeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
						vBCST = 0,
						pICMSST = 0,
						vICMSST = 0,
						pCredSN = config.AliquotaICMS > 0 ? config.AliquotaICMS : 0m,
						vCredICMSSN = config.AliquotaICMS > 0
							? Math.Round(item.Value * item.Amount * config.AliquotaICMS / 100, 2)
							: 0m
					};

				case Csosnicms.Csosn202:
					return new ICMSSN202
					{
						orig = OrigemMercadoria.OmNacional,
						CSOSN = Csosnicms.Csosn202,
						modBCST = DeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
						vBCST = 0,
						pICMSST = 0,
						vICMSST = 0
					};

				case Csosnicms.Csosn203:
					return new ICMSSN202
					{
						orig = OrigemMercadoria.OmNacional,
						CSOSN = Csosnicms.Csosn203,
						modBCST = DeterminacaoBaseIcmsSt.DbisMargemValorAgregado,
						vBCST = 0,
						pICMSST = 0,
						vICMSST = 0
					};

				case Csosnicms.Csosn300:
					return new ICMSSN102
					{
						orig = OrigemMercadoria.OmNacional,
						CSOSN = Csosnicms.Csosn300
					};

				case Csosnicms.Csosn400:
					return new ICMSSN102
					{
						orig = OrigemMercadoria.OmNacional,
						CSOSN = Csosnicms.Csosn400
					};

				case Csosnicms.Csosn500:
					return new ICMSSN500
					{
						orig = OrigemMercadoria.OmNacional,
						CSOSN = Csosnicms.Csosn500
					};

				case Csosnicms.Csosn900:
					return new ICMSSN900
					{
						orig = OrigemMercadoria.OmNacional,
						CSOSN = Csosnicms.Csosn900,
						modBC = DeterminacaoBaseIcms.DbiValorOperacao,
						vBC = item.Value * item.Amount,
						pICMS = config.AliquotaICMS > 0 ? config.AliquotaICMS : null,
						vICMS = config.AliquotaICMS > 0
							? Math.Round(item.Value * item.Amount * config.AliquotaICMS / 100, 2)
							: null,
						pCredSN = config.AliquotaICMS > 0 ? config.AliquotaICMS : null,
						vCredICMSSN = config.AliquotaICMS > 0
							? Math.Round(item.Value * item.Amount * config.AliquotaICMS / 100, 2)
							: null
					};

				default:
					return new ICMSSN102
					{
						orig = OrigemMercadoria.OmNacional,
						CSOSN = Csosnicms.Csosn102
					};
			}
		}
		protected virtual total GetTotal(VersaoServico versao, List<det> produtos, ModeloDocumento modeloDocumento, TributacaoResolvida tributacao)
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
				var tipoIcms =  produto.imposto.ICMS?.TipoICMS;
				if (tipoIcms != null)
				{
					// ── Acumula BC e ICMS por tipo ──
					switch (tipoIcms)
					{
						case ICMS00 icms00:
							icmsTot.vBC += icms00.vBC;
							icmsTot.vICMS += icms00.vICMS;
							icmsTot.vFCP = (icmsTot.vFCP ?? 0) + (icms00.vFCP ?? 0);
							break;

						case ICMS10 icms10:
							icmsTot.vBC += icms10.vBC;
							icmsTot.vICMS += icms10.vICMS;
							icmsTot.vBCST += icms10.vBCST;
							icmsTot.vST += icms10.vICMSST;
							icmsTot.vFCP = (icmsTot.vFCP ?? 0) + (icms10.vFCP ?? 0);
							icmsTot.vFCPST = (icmsTot.vFCPST ?? 0) + (icms10.vFCPST ?? 0);
							break;

						case ICMS20 icms20:
							icmsTot.vBC += icms20.vBC;
							icmsTot.vICMS += icms20.vICMS;
							icmsTot.vICMSDeson = (icmsTot.vICMSDeson ?? 0) + (icms20.vICMSDeson ?? 0);
							icmsTot.vFCP = (icmsTot.vFCP ?? 0) + (icms20.vFCP ?? 0);
							break;

						case ICMS30 icms30:
							icmsTot.vBCST += icms30.vBCST;
							icmsTot.vST += icms30.vICMSST;
							icmsTot.vICMSDeson = (icmsTot.vICMSDeson ?? 0) + (icms30.vICMSDeson ?? 0);
							icmsTot.vFCPST = (icmsTot.vFCPST ?? 0) + (icms30.vFCPST ?? 0);
							break;

						case ICMS40 icms40:
							icmsTot.vICMSDeson = (icmsTot.vICMSDeson ?? 0) + (icms40.vICMSDeson ?? 0);
							break;

						case ICMS51 icms51:
							icmsTot.vBC += icms51.vBC ?? 0;
							icmsTot.vICMS += icms51.vICMS ?? 0;
							icmsTot.vFCP = (icmsTot.vFCP ?? 0) + (icms51.vFCP ?? 0);
							break;

						case ICMS60 icms60:
							// ICMS60: ST retido anteriormente — acumula como vST
							icmsTot.vST += icms60.vICMSSTRet ?? 0;
							icmsTot.vFCPSTRet = (icmsTot.vFCPSTRet ?? 0) + (icms60.vFCPSTRet ?? 0);
							break;

						case ICMS70 icms70:
							icmsTot.vBC += icms70.vBC;
							icmsTot.vICMS += icms70.vICMS;
							icmsTot.vBCST += icms70.vBCST;
							icmsTot.vST += icms70.vICMSST;
							icmsTot.vICMSDeson = (icmsTot.vICMSDeson ?? 0) + (icms70.vICMSDeson ?? 0);
							icmsTot.vFCP = (icmsTot.vFCP ?? 0) + (icms70.vFCP ?? 0);
							icmsTot.vFCPST = (icmsTot.vFCPST ?? 0) + (icms70.vFCPST ?? 0);
							break;

						case ICMS90 icms90:
							icmsTot.vBC += icms90.vBC ?? 0;
							icmsTot.vICMS += icms90.vICMS ?? 0;
							icmsTot.vBCST += icms90.vBCST ?? 0;
							icmsTot.vST += icms90.vICMSST ?? 0;
							icmsTot.vICMSDeson = (icmsTot.vICMSDeson ?? 0) + (icms90.vICMSDeson ?? 0);
							icmsTot.vFCP = (icmsTot.vFCP ?? 0) + (icms90.vFCP ?? 0);
							icmsTot.vFCPST = (icmsTot.vFCPST ?? 0) + (icms90.vFCPST ?? 0);
							break;

						case ICMSSN101:
							// CSOSN 101: apenas crédito SN, sem BC/ICMS próprios
							break;

						case ICMSSN102:
							// CSOSN 102: sem valores a acumular (sem crédito)
							break;

						case ICMSSN201 sn201:
							icmsTot.vBCST += sn201.vBCST;
							icmsTot.vST += sn201.vICMSST;
							break;

						case ICMSSN202 sn202:
							icmsTot.vBCST += sn202.vBCST;
							icmsTot.vST += sn202.vICMSST;
							break;

						case ICMSSN500 sn500:
							// SN 500: ST retido anteriormente — acumula como vST
							icmsTot.vST += sn500.vICMSSTRet ?? 0;
							icmsTot.vFCPSTRet = (icmsTot.vFCPSTRet ?? 0) + (sn500.vFCPSTRet ?? 0);
							break;

						case ICMSSN900 sn900:
							icmsTot.vBC += sn900.vBC ?? 0;
							icmsTot.vICMS += sn900.vICMS ?? 0;
							icmsTot.vBCST += sn900.vBCST ?? 0;
							icmsTot.vST += sn900.vICMSST ?? 0;
							icmsTot.vFCPST = (icmsTot.vFCPST ?? 0) + (sn900.vFCPST ?? 0);
							break;
					}
				}

				// ── Acumula IPI ──
				if (produto.imposto.IPI?.TipoIPI is IPITrib ipiTrib)
				{
					icmsTot.vIPI += ipiTrib.vIPI ?? 0;
				}

				// ── Acumula II ──
				if (produto.imposto.II != null)
				{
					icmsTot.vII += produto.imposto.II.vBC > 0 ? produto.imposto.II.vII : 0;
				}
			}

			// ** Cálculo do vNF conforme regra de validação W16-10 **
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
				ICMSTot = icmsTot
			};

			// ── IS Tot (apenas NFe, soma dos IS dos itens) ──
			if (modeloDocumento == ModeloDocumento.NFe)
			{
				decimal totalIS = produtos.Sum(p =>
				{
					if (p.imposto.IS != null)
						return p.imposto.IS.vIS;
					return 0m;
				});

				if (totalIS > 0)
				{
					t.ISTot = new ISTot { vIS = totalIS };
				}

				// ── ISSQN Tot (apenas NFe) ──
				decimal totalISSQN = produtos.Sum(p =>
				{
					if (p.imposto.ISSQN != null)
						return p.imposto.ISSQN.vISSQN;
					return 0m;
				});

				if (totalISSQN > 0)
				{
					t.ISSQNtot = new ISSQNtot
					{
						vBC = icmsTot.vProd,
						vISS = Math.Round(totalISSQN, 2)
					};
				}
			}

			// ── IBS/CBS Tot (obrigatório para NFe e NFCe a partir de 2026 - NT 2025.002) ──
			{
				decimal totalBCIBSCBS = icmsTot.vProd;
				decimal totalIBS = produtos.Sum(p =>
					p.imposto?.IBSCBS?.gIBSCBS?.vIBS ?? 0);
				decimal totalIBSUF = produtos.Sum(p =>
					p.imposto?.IBSCBS?.gIBSCBS?.gIBSUF?.vIBSUF ?? 0);
				decimal totalIBSMun = produtos.Sum(p =>
					p.imposto?.IBSCBS?.gIBSCBS?.gIBSMun?.vIBSMun ?? 0);
				decimal totalCBS = produtos.Sum(p =>
					p.imposto?.IBSCBS?.gIBSCBS?.gCBS?.vCBS ?? 0);

				//t.IBSCBSTot = new IBSCBSTot
				//{
				//	vBCIBSCBS = totalBCIBSCBS,
				//	gIBS = new gIBSTotal
				//	{
				//		vIBS = Math.Round(totalIBS, 2),
				//		gIBSUF = new gIBSUFTotal
				//		{
				//			vIBSUF = Math.Round(totalIBSUF, 2),
				//			vDif = 0,
				//			vDevTrib = 0
				//		},
				//		gIBSMun = new gIBSMunTotal
				//		{
				//			vIBSMun = Math.Round(totalIBSMun, 2),
				//			vDif = 0,
				//			vDevTrib = 0
				//		},
				//		vCredPres = 0,
				//		vCredPresCondSus = 0
				//	},
				//	gCBS = new gCBSTotal
				//	{
				//		vCBS = Math.Round(totalCBS, 2),
				//		vDif = 0,
				//		vDevTrib = 0,
				//		vCredPres = 0,
				//		vCredPresCondSus = 0
				//	}
				//};
			}

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
				ide.xJust = "CONTIG�NCIA PARA NFe/NFCe";
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
			Console.WriteLine($"DataEmiss�o>>>>>>>>>>{DateTime.Now}");
			Console.WriteLine($"DataEmiss�oUTC>>>>>>>>>>{DateTime.UtcNow}");
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
			var emit = _configuracaoApp.Emitente;
			emit.enderEmit = GetEnderecoEmitente();
			return emit;
		}

		protected virtual enderEmit GetEnderecoEmitente()
		{
			var enderEmit = _configuracaoApp.EnderecoEmitente; 
			enderEmit.cPais = 1058;
			enderEmit.xPais = "BRASIL";
			return enderEmit;
		}
		protected virtual dest GetDestinatario(VersaoServico versao, ModeloDocumento modelo)
		{
			var dest = new dest(versao);

			// VERIFICA��O PRINCIPAL: Quando o cliente � NULL
			if (_currentSale?.Client == null)
			{
				// Consumidor final n�o identificado
				//dest.idEstrangeiro = "ext";
				// Nome padr�o para consumidor n�o identificado
				dest.CNPJ = "99999999000191";
				dest.xNome = _currentFiscalConfiguration.Ambiente == AmbienteEnum.Homologacao
						? "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL"
						: "CONSUMIDOR NAO IDENTIFICADO";

				// Indicador de IE: 9 = N�o Contribuinte (padr�o para consumidor final)
				dest.indIEDest = indIEDest.NaoContribuinte;
				//dest.IE = null;
				// Endere�o padr�o
				dest.enderDest = GetEnderecoDestinatarioPadrao();

				return dest;

			}

			// Cliente existe - fluxo normal
			// Configura��o do documento (CPF ou CNPJ)
			if (_currentSale.Client.TipoPessoa == "J")
			{
				dest.CNPJ = _currentSale.Client.Document;
			}
			else
			{
				dest.CPF = _currentSale?.Client?.Document ?? "99999999999";
			}

			// Nome do destinat�rio
			dest.xNome = _currentFiscalConfiguration.Ambiente == AmbienteEnum.Homologacao
							? "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL"
							: _currentSale?.Client?.Name ?? "CONSUMIDOR NAO IDENTIFICADO";

			// Endere�o do destinat�rio
			dest.enderDest = GetEnderecoDestinatario();

			// Configura��es espec�ficas por vers�o
			if (versao == VersaoServico.Versao200)
			{
				if (!string.IsNullOrEmpty(_currentSale?.Client?.Ie))
					dest.IE = _currentSale.Client.Ie;
				else
					dest.IE = "ISENTO";
				return dest;
			}

			// Para vers�o 3.00 e superiores
			if (_currentSale?.Client?.IndicadorIE != null)
			{
				switch (_currentSale.Client.IndicadorIE)
				{
					case "1":
						dest.indIEDest = indIEDest.ContribuinteICMS;
						if (!string.IsNullOrEmpty(_currentSale.Client.Ie))
							dest.IE = _currentSale.Client.Ie;
						break;
					case "2":
						dest.indIEDest = indIEDest.Isento;
						dest.IE = "ISENTO";
						break;
					case "9":
						dest.indIEDest = indIEDest.NaoContribuinte;
						break;
				}
			}
			else
			{
				dest.indIEDest = indIEDest.NaoContribuinte;
			}

			if (!string.IsNullOrEmpty(_currentSale?.Client?.Email))
				dest.email = _currentSale.Client.Email;
			else if (_currentFiscalConfiguration.Ambiente == AmbienteEnum.Homologacao)
				dest.email = "homologacao@sefaz.gov.br";

			return dest;
		}

		protected virtual enderDest GetEnderecoDestinatarioPadrao()
		{
			return new enderDest
			{
				xLgr = "ENDERECO NAO INFORMADO",
				nro = "S/N",
				xCpl = null,
				xBairro = "CENTRO",
				cMun = _configuracaoApp.EnderecoEmitente.cMun,  // C�digo gen�rico
				xMun = "NAO INFORMADO",
				UF = _configuracaoApp.EnderecoEmitente.UF.ToString(),
				CEP = "00000000",
				cPais = 1058,
				xPais = "BRASIL"
			};
		}

		protected virtual enderDest GetEnderecoDestinatario()
		{
			var endereco = new enderDest();
			var cliente = _currentSale?.Client;

			// VERIFICA��O: Se cliente � nulo, retorna endere�o padr�o
			if (cliente == null)
			{
				return GetEnderecoDestinatarioPadrao();
			}

			try
			{
				endereco.xLgr = cliente.Address ?? "ENDERECO NAO INFORMADO";
				endereco.nro = cliente.Numero ?? "S/N";
				endereco.xCpl = cliente.Complemento;
				endereco.xBairro = cliente.Bairro ?? "CENTRO";

				// Valida��o para evitar erros de parse
				if (!string.IsNullOrEmpty(cliente.CodMunicipioIbge))
					endereco.cMun = long.Parse(cliente.CodMunicipioIbge);
				else
					endereco.cMun = 9999999;

				endereco.xMun = cliente.Municipio ?? "NAO INFORMADO";
				endereco.UF = cliente.Uf ?? "SP";
				endereco.CEP = cliente.ZipCode ?? "00000000";

				if (!string.IsNullOrEmpty(cliente.CodPais))
					endereco.cPais = int.Parse(cliente.CodPais);
				else
					endereco.cPais = 1058;

				endereco.xPais = cliente.Pais ?? "Brasil";

				if (!string.IsNullOrEmpty(cliente.CellPhone))
				{
					var telefone = new string(cliente.CellPhone.Where(char.IsDigit).ToArray());
					if (telefone.Length >= 10)
					{
						endereco.fone = long.Parse(telefone);
					}
				}
			}
			catch (Exception)
			{
				return GetEnderecoDestinatarioPadrao();
			}

			return endereco;
		}
		protected virtual transp GetTransporte()
		{
			var t = new transp
			{
				modFrete = ModalidadeFrete.mfSemFrete //NFCe: N�o pode ter frete
																							//vol = volumes 
			};

			return t;
		}

		public async Task UpdateResultAsync(int id, bool sent, long? numero, string? responseJson, string? errorMessage)
		{
			var existing = await repository.GetByIdAsync(id);
			if (existing == null) throw new InvalidOperationException("Registro NFe n�o encontrado.");

			existing.Sent = sent;
			existing.Numero = numero ?? existing.Numero;
			existing.ResponseJson = responseJson;
			existing.ErrorMessage = errorMessage;
			existing.TryCount += 1;
			existing.UpdatedAt = DateTime.Now;

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

				if (nFeEmission.TipoDocumento == TipoDocumentoEnum.NFE)
				{
					var configuracaoDanfeNfe = new NFe.Danfe.Base.NFe.ConfiguracaoDanfeNfe(null, true, false, true);
					var danfeDocument = new DanfeNfeDocument(arquivo, null/*logoBytes*/, configuracaoDanfeNfe);
					var pdfBytes = danfeDocument.GerarPdfBytes();
					return pdfBytes;
				}
				else
				{
					var danfeDocument = new DanfeNfceDocument(arquivo, null/*logoBytes*/);
					danfeDocument.TamanhoImpressao(NFe.Danfe.QuestPdf.ImpressaoNfce.TamanhoImpressao.Impressao80);
					var pdfBytes = danfeDocument.GerarPdfBytes();
					return pdfBytes;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("=== EXCEPTION NA DANFE===");
				Console.WriteLine(ex.ToString());
				return [];
			}
		}
		// M�todo de extens�o para IDocument

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
			if (nFeEmission == null) return new ResponseGeneric { Success = false, Message = "NFe n�o encontrada." };
			try
			{
				
				FiscalConfiguration fiscalConfiguration = await _fiscalConfigurationRepository.GetByCompany(nFeEmission.CompanyId);
				NaturezaOperacao naturezaOperacao = await _naturezaOperacaoRepository.GetByIdAsync(nFeEmission.NaturezaOperacaoId);

			
				_configuracaoApp = criarConfiguracaoApp(fiscalConfiguration, naturezaOperacao);
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
					nFeEmission.UpdatedAt = DateTime.Now;
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
		Task<ResponseGeneric> Resend(int id, int? nop);
		Task<byte[]> Danfe(int id);
		Task update(NFeEmissionDto attempt);
		Task<byte[]> ObterXml(int id);
		Task<string> ObterNomeArquivoXml(int id);
		Task<ResponseGeneric> CancelarNfe(CancelarNotaRequest cancelarNota);
		Task<ResponseGeneric> CreatedFromSale(NFeEmissionDto nfeDto);
	}
}
