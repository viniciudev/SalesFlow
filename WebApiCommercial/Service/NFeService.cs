using DFe.Classes.Entidades;
using DFe.Classes.Flags;
using Microsoft.AspNetCore.Hosting;
using Model;
using Model.DTO;
using Model.Enums;
using Model.Moves;
using Model.Registrations;
using Newtonsoft.Json;
using NFe.AppTeste;
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
using NFe.Utils.InformacoesSuplementares;
using NFe.Utils.NFe;
using Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private ConfiguracaoApp _configuracaoApp;
        private FiscalConfiguration _currentFiscalConfiguration;
        private NaturezaOperacao _currentNaturezaOperacao;

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
        public async Task<ResponseGeneric> Resend(int id)
        {
            NFeEmission nFeEmission = await (repository as INFeRepository).GetByIdAsync(id);
            if (nFeEmission == null)
                return new ResponseGeneric { Success = false, Message = "Não foi encontrado a nota!" };
            //configuraçao da empresa para nfe
            FiscalConfiguration fiscalConfiguration = await _fiscalConfigurationRepository.GetByCompany(nFeEmission.CompanyId);
            if (fiscalConfiguration == null)
                return new ResponseGeneric { Success = false, Message = "Não encontrado as configurações para emissão de nota!" };
            //verifica se existe a venda
            Sale sale = await _saleRepository.GetSaleByCompany(nFeEmission.SaleId, nFeEmission.CompanyId);
            if (sale == null)
                return new ResponseGeneric { Success = false, Message = "Venda não encontrada para a empresa." };

            NaturezaOperacao naturezaOperacao = await _naturezaOperacaoRepository.GetByIdAsync(nFeEmission.NaturezaOperacaoId);
            if (naturezaOperacao == null)
                return new ResponseGeneric { Success = false, Message = "Natureza de operação não encontrada." };

       
            //classes externas para gerar nfe
            var respEmissao = await TransmitirNfe(Convert.ToInt32( nFeEmission.Numero), nFeEmission, fiscalConfiguration, sale, naturezaOperacao);
            if (respEmissao is string mensagemErro)
            {
                //mudar status
                //mensagem de erro
                nFeEmission.Sent = true;
                nFeEmission.Numero = nFeEmission.Numero;
                nFeEmission.StatusNfe = StatusNfe.pendente;
                //nFeEmission.ResponseJson = responseJson;
                nFeEmission.ErrorMessage = respEmissao;
                nFeEmission.Serie = fiscalConfiguration.NumeracaoDocumentos.Nfce.Serie;
                nFeEmission.TryCount += 1;
                nFeEmission.UpdatedAt = DateTime.UtcNow;
               
            }
            else if (respEmissao is RetornoNFeAutorizacao ret)
            {
                protNFe protNFe = ret.Retorno.protNFe;
                infProt infProt = protNFe?.infProt;
                //mensagem de erro
                nFeEmission.Sent = true;
                nFeEmission.Numero = nFeEmission.Numero;
                nFeEmission.StatusNfe = NfeSituacao.Autorizada(infProt.cStat)? StatusNfe.emitida: StatusNfe.pendente;
                nFeEmission.ResponseJson = JsonConvert.SerializeObject (infProt);
                nFeEmission.ErrorMessage = NfeSituacao.Autorizada(infProt.cStat) ?null: infProt.xMotivo;
                nFeEmission.Serie = fiscalConfiguration.NumeracaoDocumentos.Nfce.Serie;
                nFeEmission.TryCount += 1;
                nFeEmission.UpdatedAt = DateTime.UtcNow;
                nFeEmission.ChaveAcesso = infProt.chNFe;
                nFeEmission.XmlCompleto = ret.EnvioStr;
            }

            await repository.UpdateAsync(nFeEmission.Id, nFeEmission);
            return new ResponseGeneric { Success = true };
        }
        public async Task<ResponseGeneric> CreateAttemptAsync(NFeEmissionDto attempt)
        {
            //ultima nota emitida com sucesso
            NFeEmission nFeEmission = await (repository as INFeRepository).GetByCompany(attempt.CompanyId);

            //configuraçao da empresa para nfe
            FiscalConfiguration fiscalConfiguration = await _fiscalConfigurationRepository.GetByCompany(attempt.CompanyId);
            if (fiscalConfiguration == null)
                return new ResponseGeneric { Success = false, Message = "Não encontrado as configurações para emissão de nota!" };

            //verifica se existe a venda
            Sale sale = await _saleRepository.GetSaleByCompany(attempt.SaleId, attempt.CompanyId);
            if (sale == null)
                return new ResponseGeneric { Success = false, Message = "Venda não encontrada para a empresa." };

            NaturezaOperacao naturezaOperacao = await _naturezaOperacaoRepository.GetByIdAsync(attempt.NaturezaOperacaoId);
            if (naturezaOperacao == null)
                return new ResponseGeneric { Success = false, Message = "Natureza de operação não encontrada." };
            int proximoNumeroNfe = Convert.ToInt32( nFeEmission == null ? fiscalConfiguration.NumeracaoDocumentos.Nfce.NumeroInicial : nFeEmission.Numero + 1);
            //classes externas para gerar nfe
            var respEmissao = await TransmitirNfe(proximoNumeroNfe, nFeEmission, fiscalConfiguration, sale, naturezaOperacao);

            attempt.TryCount = attempt.TryCount <= 0 ? 1 : attempt.TryCount;
            attempt.CreatedAt = DateTime.UtcNow;
            var entity = new NFeEmission();
            if (respEmissao is string mensagemErro)
            {
                entity.ResponseJson = "";
                entity.ErrorMessage = mensagemErro;
                entity.NaturezaOperacaoId = attempt.NaturezaOperacaoId;
                entity.SaleId = attempt.SaleId;
                entity.TipoDocumento = attempt.TipoDocumento;
                entity.Serie = fiscalConfiguration.NumeracaoDocumentos.Nfce.Serie;
                entity.Numero = proximoNumeroNfe;
                entity.StatusNfe = StatusNfe.pendente;
                entity.CreatedAt = DateTime.UtcNow;
                entity.TryCount = attempt.TryCount;
                entity.CompanyId = attempt.CompanyId;
            }
            else if(respEmissao is RetornoNFeAutorizacao ret)
            {
                protNFe protNFe = ret.Retorno.protNFe;
                infProt infProt = protNFe?.infProt;
                entity.ResponseJson = JsonConvert.SerializeObject(infProt);
                entity.ErrorMessage= NfeSituacao.Autorizada(infProt.cStat) ? null : infProt.xMotivo;
                entity.NaturezaOperacaoId = attempt.NaturezaOperacaoId;
                entity.SaleId = attempt.SaleId;
                entity.TipoDocumento = attempt.TipoDocumento;
                entity.Serie = fiscalConfiguration.NumeracaoDocumentos.Nfce.Serie;
                entity.Numero = proximoNumeroNfe;
                entity.StatusNfe = NfeSituacao.Autorizada(infProt.cStat) ? StatusNfe.emitida : StatusNfe.pendente; ;
                entity.CreatedAt = DateTime.UtcNow;
                entity.TryCount = attempt.TryCount;
                entity.CompanyId = attempt.CompanyId;
                entity.XmlCompleto = ret.RetornoCompletoStr;
            }

              
            await repository.CreateAsync(entity);
            return new ResponseGeneric { Success = true };
        }

        private async Task<dynamic> TransmitirNfe(int numero, NFeEmission nFeEmission, FiscalConfiguration fiscalConfiguration, Sale sale, NaturezaOperacao naturezaOperacao)
        {
            try
            {
                //var numero = Funcoes.InpuBox(this, "Criar e Enviar NFe", "Número da Nota:");
                //if (string.IsNullOrEmpty(numero)) throw new Exception("O Número deve ser informado!");
                byte[] certbyte = await ObterCertificado(fiscalConfiguration.CertificadoDigital.Arquivo);
                _currentFiscalConfiguration = fiscalConfiguration;
                _currentNaturezaOperacao = naturezaOperacao;
                _configuracaoApp = criarConfiguracaoApp(fiscalConfiguration, naturezaOperacao, certbyte);
                _nfe = ObterNfeValidada(VersaoServico.Versao400, ModeloDocumento.NFCe,
                    numero, new ConfiguracaoCsc
                    {
                        CIdToken = fiscalConfiguration.Csc.Identificador,
                        Csc = fiscalConfiguration.Csc.Valor
                    });
                var servicoNFe = new ServicosNFe(_configuracaoApp.CfgServico);
                var retornoEnvio = servicoNFe.NFeAutorizacao(int.Parse(fiscalConfiguration.NumeracaoDocumentos.Nfce.Serie), IndicadorSincronizacao.Sincrono, new List<NFe.Classes.NFe> { _nfe }, false/*Envia a mensagem compactada para a SEFAZ*/);
   /*             var resp=OnSucessoSync(retornoEnvio)*/;
               
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
            // O caminho relativo vindo do banco: "/certs/1918d285-171a-4cf7-8c07-27a06481fbd4.pfx"
            // Remove a barra inicial se necessário
            caminhoRelativo = caminhoRelativo.TrimStart('/');

            // Combina com o caminho da wwwroot
            string caminhoCompleto = Path.Combine(_environment.WebRootPath, caminhoRelativo);

            // Verifica se o arquivo existe
            if (!System.IO.File.Exists(caminhoCompleto))
            {
                throw new FileNotFoundException($"Certificado não encontrado: {caminhoCompleto}");
            }

            // Lê o arquivo
            return await System.IO.File.ReadAllBytesAsync(caminhoCompleto);
        }
        private ConfiguracaoApp criarConfiguracaoApp(FiscalConfiguration fiscalConfiguration,NaturezaOperacao naturezaOperacao,
            byte[] certbyte)
        {
            try
            {


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
                        ModeloDocumento =naturezaOperacao.TipoDocumento==TipoDocumentoEnum.NFCE?ModeloDocumento.NFCe:ModeloDocumento.NFe,
                        VersaoNFeAutorizacao =VersaoServico.Versao400,
                         VersaoNFeRetAutorizacao=VersaoServico.Versao400,
                         VersaoLayout=VersaoServico.Versao400,
                         cUF= (Estado)Enum.Parse(typeof(Estado), fiscalConfiguration.Emitente.EmitenteEndereco.Uf),
                        ProtocoloDeSeguranca=System.Net.SecurityProtocolType.Tls12,
                        VersaoConsultaGTIN=VersaoServico.Versao400,
                        VersaoNfceAministracaoCSC=VersaoServico.Versao400,
                        VersaoNfeDownloadNF=VersaoServico.Versao400,
                        VersaoNfeRecepcao=VersaoServico.Versao400,
                        ValidarSchemas=true,
                        RemoverAcentos=true,
                        DiretorioSchemas = DiretorioSchemas,
                        tpAmb = fiscalConfiguration.Ambiente == AmbienteEnum.Homologacao ?
                                    TipoAmbiente.Homologacao : TipoAmbiente.Producao,
                        tpEmis = TipoEmissao.teNormal,
                        Certificado = new DFe.Utils.ConfiguracaoCertificado
                        {
                            TipoCertificado = DFe.Utils.TipoCertificado.A1ByteArray,
                            ArrayBytesArquivo = certbyte,
                            Senha = fiscalConfiguration.CertificadoDigital.Senha,
                            ManterDadosEmCache = false
                        }
                    },
                    Emitente = new emit
                    {
                        CNPJ =LimparString( fiscalConfiguration.Emitente.Cnpj),
                        IE = LimparString( fiscalConfiguration.Emitente.InscricaoEstadual),
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

            for (var i = 0; i < 5; i++)
            {
                infNFe.det.Add(GetDetalhe(i, infNFe.emit.CRT, modelo));
            }

            infNFe.total = GetTotal(versao, infNFe.det,modelo);

            if (infNFe.ide.mod == ModeloDocumento.NFe & (versao == VersaoServico.Versao310 || versao == VersaoServico.Versao400))
                infNFe.cobr = GetCobranca(infNFe.total.ICMSTot); //V3.00 e 4.00 Somente
            if (infNFe.ide.mod == ModeloDocumento.NFCe || (infNFe.ide.mod == ModeloDocumento.NFe & versao == VersaoServico.Versao400))
                infNFe.pag = GetPagamento(infNFe.total.ICMSTot, versao); //NFCe Somente  

            if (infNFe.ide.mod == ModeloDocumento.NFCe & versao != VersaoServico.Versao400)
                infNFe.infAdic = new infAdic() { infCpl = "Troco: 10,00" }; //Susgestão para impressão do troco em NFCe

            return infNFe;
        }
        protected virtual List<pag> GetPagamento(ICMSTot icmsTot, VersaoServico versao)
        {
            var valorPagto = (icmsTot.vNF / 2).Arredondar(2);

            if (versao != VersaoServico.Versao400) // difernte de versão 4 retorna isso
            {
                var p = new List<pag>
                {
                    new pag {tPag = FormaPagamento.fpDinheiro, vPag = valorPagto},
                    new pag {tPag = FormaPagamento.fpCheque, vPag = icmsTot.vNF - valorPagto}
                };
                return p;
            }


            // igual a versão 4 retorna isso
            var p4 = new List<pag>
            {
                //new pag {detPag = new detPag {tPag = FormaPagamento.fpDinheiro, vPag = valorPagto}},
                //new pag {detPag = new detPag {tPag = FormaPagamento.fpCheque, vPag = icmsTot.vNF - valorPagto}}
                new pag
                {
                    detPag = new List<detPag>
                    {
                        new detPag {tPag = FormaPagamento.fpDinheiro, vPag = valorPagto},
                        new detPag {tPag = FormaPagamento.fpCheque, vPag = icmsTot.vNF - valorPagto}
                    }
                }
            };


            return p4;
        }
        protected virtual cobr GetCobranca(ICMSTot icmsTot)
        {
            var valorParcela = (icmsTot.vNF / 2).Arredondar(2);
            var c = new cobr
            {
                fat = new fat { nFat = "12345678910", vLiq = icmsTot.vNF, vOrig = icmsTot.vNF, vDesc = 0m },
                dup = new List<dup>
                {
                    new dup {nDup = "001", dVenc = DateTime.Now.AddDays(30), vDup = valorParcela},
                    new dup {nDup = "002", dVenc = DateTime.Now.AddDays(60), vDup = icmsTot.vNF - valorParcela}
                }
            };

            return c;
        }
        protected virtual prod GetProduto(int i)
        {
            var p = new prod
            {
                cProd = i.ToString().PadLeft(5, '0'),
                cEAN = "7770000000012",
                xProd = i == 1 ? "NOTA FISCAL EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL" : "ABRACADEIRA NYLON 6.6 BRANCA 91X92 " + i,
                NCM = "84159090",
                CFOP = 5102,
                uCom = "UNID",
                qCom = 1,
                vUnCom = 1.1m,
                vProd = 1.1m,
                vDesc = 0.10m,
                cEANTrib = "7770000000012",
                uTrib = "UNID",
                qTrib = 1,
                vUnTrib = 1.1m,
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
        protected virtual det GetDetalhe(int i, CRT crt, ModeloDocumento modelo)
        {
            var produto = GetProduto(i + 1);

            var det = new det
            {
                nItem = i + 1,
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
                        TipoCOFINS = new COFINSOutr { CST = CSTCOFINS.cofins99, pCOFINS = 0, vBC = 0, vCOFINS = 0 }
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
        protected virtual total GetTotal(VersaoServico versao, List<det> produtos,ModeloDocumento modeloDocumento)
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
                natOp = "VENDA",
                mod = modelo,
                serie = 1,
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
                ide.xJust = "TESTE DE CONTIGÊNCIA PARA NFe/NFCe";
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
            var dest = new dest(versao)
            {
                CNPJ = "99999999000191",
                //CPF = "99999999999",
            };
            dest.xNome = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL"; //Obrigatório para NFe e opcional para NFCe
            dest.enderDest = GetEnderecoDestinatario(); //Obrigatório para NFe e opcional para NFCe

            //if (versao == VersaoServico.Versao200)
            //    dest.IE = "ISENTO";
            if (versao == VersaoServico.Versao200) return dest;

            dest.indIEDest = indIEDest.NaoContribuinte; //NFCe: Tem que ser não contribuinte V3.00 Somente
            dest.email = "teste@gmail.com"; //V3.00 Somente
            return dest;
        }
        protected virtual enderDest GetEnderecoDestinatario()
        {
            var enderDest = new enderDest
            {
                xLgr = "RUA ...",
                nro = "S/N",
                xBairro = "CENTRO",
                cMun = 2802908,
                xMun = "ITABAIANA",
                UF = "SE",
                CEP = "49500000",
                cPais = 1058,
                xPais = "BRASIL"
            };
            return enderDest;
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
            string arquivoXml = nFeEmission.XmlCompleto;//Funcoes.BuscarArquivoXml();
            try
            {
                nfeProc proc = null;
                NFe.Classes.NFe nfe = null;
                string arquivo = string.Empty;

                try
                {
                    proc = new nfeProc().CarregarDeArquivoXml(arquivoXml);
                    arquivo = proc.ObterXmlString();
                }
                catch (Exception)
                {
                    nfe = new NFe.Classes.NFe().CarregarDeArquivoXml(arquivoXml);
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
        public async Task update(NFeEmissionDto attempt)
        {
            NFeEmission nFeEmission = await GetByIdAsync(attempt.Id);
            if (nFeEmission == null) return;
            nFeEmission.NaturezaOperacaoId = attempt.NaturezaOperacaoId;
            nFeEmission.SaleId = attempt.SaleId;
            nFeEmission.Numero = attempt.Numero;

            await repository.UpdateAsync(nFeEmission.Id, nFeEmission);
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
    }
}