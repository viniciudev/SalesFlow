using DFe.Classes.Flags;
using Model;
using Model.DTO;
using Model.Enums;
using Model.Moves;
using Model.Registrations;
using NFe.AppTeste;
using NFe.Classes;
using NFe.Classes.Informacoes;
using NFe.Classes.Informacoes.Identificacao;
using NFe.Utils;
using Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Repository.NFeRepository;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Service
{
    public class NFeService : BaseService<NFeEmission>, INFeService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IFiscalConfigurationRepository _fiscalConfigurationRepository;
        private readonly INaturezaOperacaoRepository _naturezaOperacaoRepository;
        private NFe.Classes.NFe _nfe;
        private ConfiguracaoApp _configuracoes;
        public NFeService(IGenericRepository<NFeEmission> repository,
            ISaleRepository saleRepository,
            IFiscalConfigurationRepository fiscalConfigurationRepository,
            INaturezaOperacaoRepository naturezaOperacaoRepository) : base(repository)
        {
            _saleRepository = saleRepository;
            _fiscalConfigurationRepository = fiscalConfigurationRepository;
            _naturezaOperacaoRepository = naturezaOperacaoRepository;
        }
       public async Task<ResponseGeneric> Resend(int id)
        {
            NFeEmission nFeEmission = await (repository as INFeRepository).GetByIdAsync(id);
            if(nFeEmission==null) 
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
            var respEmissao = await TransmitirNfe(nFeEmission, fiscalConfiguration, sale, naturezaOperacao);

            //mudar status
            //mensagem de erro
            nFeEmission.Sent = true;
            //nFeEmission.Numero = numero ?? existing.Numero;
            //nFeEmission.ResponseJson = responseJson;
            //nFeEmission.ErrorMessage = errorMessage;
            nFeEmission.TryCount += 1;
            nFeEmission.UpdatedAt = DateTime.UtcNow;

            await repository.UpdateAsync(nFeEmission.Id,nFeEmission);
            return new ResponseGeneric { Success = true };
        }
        public async Task<ResponseGeneric> CreateAttemptAsync(NFeEmissionDto attempt)
        {
            //ultima nota emitida com sucesso
            NFeEmission nFeEmission= await  (repository as INFeRepository).GetByCompany(attempt.CompanyId);

            //configuraçao da empresa para nfe
            FiscalConfiguration fiscalConfiguration =await _fiscalConfigurationRepository.GetByCompany(attempt.CompanyId);
            if(fiscalConfiguration==null)
                return new ResponseGeneric { Success = false, Message = "Não encontrado as configurações para emissão de nota!" };
            //verifica se existe a venda
            Sale sale = await _saleRepository.GetSaleByCompany(attempt.SaleId,attempt.CompanyId);
            if(sale==null)
                return new ResponseGeneric { Success=false,Message= "Venda não encontrada para a empresa." };

            NaturezaOperacao naturezaOperacao= await _naturezaOperacaoRepository.GetByIdAsync(attempt.NaturezaOperacaoId);
            if (naturezaOperacao == null)
                return new ResponseGeneric { Success = false, Message = "Natureza de operação não encontrada." };

            //classes externas para gerar nfe
            var respEmissao = await TransmitirNfe(nFeEmission,fiscalConfiguration,sale,naturezaOperacao);

            attempt.TryCount = attempt.TryCount <= 0 ? 1 : attempt.TryCount;
            attempt.CreatedAt = DateTime.UtcNow;

                var entity = new NFeEmission
                {
                    ResponseJson= respEmissao,
                    NaturezaOperacaoId = attempt.NaturezaOperacaoId,
                    SaleId = attempt.SaleId,
                    TipoDocumento = attempt.TipoDocumento,
                    Serie = fiscalConfiguration.NumeracaoDocumentos.Nfce.Serie,
                    Numero = nFeEmission==null?fiscalConfiguration.NumeracaoDocumentos.Nfce.NumeroInicial:nFeEmission.Numero+1,
                    StatusNfe = attempt.StatusNfe,
                    CreatedAt = attempt.CreatedAt,
                    TryCount = attempt.TryCount,
                    CompanyId = attempt.CompanyId
                };
            await repository.CreateAsync(entity);
            return new ResponseGeneric { Success=true};
        }

        private async Task<string> TransmitirNfe(NFeEmission nFeEmission, FiscalConfiguration fiscalConfiguration, Sale sale, NaturezaOperacao naturezaOperacao)
        {
            try
            {
                //var numero = Funcoes.InpuBox(this, "Criar e Enviar NFe", "Número da Nota:");
                //if (string.IsNullOrEmpty(numero)) throw new Exception("O Número deve ser informado!");

                _nfe = ObterNfeValidada(VersaoServico.Versao400, ModeloDocumento.NFCe,
                    Convert.ToInt32(nFeEmission.Numero), new ConfiguracaoCsc
                    {
                        CIdToken = fiscalConfiguration.Csc.Identificador,
                        Csc = fiscalConfiguration.Csc.Valor
                    });

                ExibeNfe();

                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = _nfe.infNFe.Id.Substring(3),
                    DefaultExt = ".xml",
                    Filter = "Arquivo XML (.xml)|*.xml"
                };
                var result = dlg.ShowDialog();
                if (result != true) return;
                var arquivoXml = dlg.FileName;
                _nfe.SalvarArquivoXml(arquivoXml);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(ex.Message))
                    Funcoes.Mensagem(ex.Message, "Erro", MessageBoxButton.OK);
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
                    nfe.infNFeSupl.urlChave = nfe.infNFeSupl.ObterUrlConsulta(nfe, _configuracoes.ConfiguracaoDanfeNfce.VersaoQrCode);
                nfe.infNFeSupl.qrCode = nfe.infNFeSupl.ObterUrlQrCode(nfe, _configuracoes.ConfiguracaoDanfeNfce.VersaoQrCode, configuracaoCsc.CIdToken, configuracaoCsc.Csc, _configuracoes.CfgServico.Certificado);
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

            infNFe.total = GetTotal(versao, infNFe.det);

            if (infNFe.ide.mod == ModeloDocumento.NFe & (versao == VersaoServico.Versao310 || versao == VersaoServico.Versao400))
                infNFe.cobr = GetCobranca(infNFe.total.ICMSTot); //V3.00 e 4.00 Somente
            if (infNFe.ide.mod == ModeloDocumento.NFCe || (infNFe.ide.mod == ModeloDocumento.NFe & versao == VersaoServico.Versao400))
                infNFe.pag = GetPagamento(infNFe.total.ICMSTot, versao); //NFCe Somente  

            if (infNFe.ide.mod == ModeloDocumento.NFCe & versao != VersaoServico.Versao400)
                infNFe.infAdic = new infAdic() { infCpl = "Troco: 10,00" }; //Susgestão para impressão do troco em NFCe

            return infNFe;
        }
        protected virtual ide GetIdentificacao(int numero, ModeloDocumento modelo, VersaoServico versao)
        {
            var ide = new ide
            {
                cUF = _configuracoes.EnderecoEmitente.UF,
                natOp = "VENDA",
                mod = modelo,
                serie = 1,
                nNF = numero,
                tpNF = TipoNFe.tnSaida,
                cMunFG = _configuracoes.EnderecoEmitente.cMun,
                tpEmis = _configuracoes.CfgServico.tpEmis,
                tpImp = TipoImpressao.tiRetrato,
                cNF = "1234",
                tpAmb = _configuracoes.CfgServico.tpAmb,
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
            return await (repository as INFeRepository ).GetAllAsync(tenantid);
        }
        public async Task<PagedResult<NFeEmission>> GetPaged(Filters filters)
        {
            return await (repository as INFeRepository).GetPaged(filters);
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
    }
}