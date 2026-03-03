using Model;
using Model.DTO;
using Model.Enums;
using Model.Moves;
using Model.Registrations;
using Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Repository.NFeRepository;

namespace Service
{
    public class NFeService : BaseService<NFeEmission>, INFeService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IFiscalConfigurationRepository _fiscalConfigurationRepository;
        private readonly INaturezaOperacaoRepository _naturezaOperacaoRepository;
        public NFeService(IGenericRepository<NFeEmission> repository,
            ISaleRepository saleRepository,
            IFiscalConfigurationRepository fiscalConfigurationRepository,
            INaturezaOperacaoRepository naturezaOperacaoRepository) : base(repository)
        {
            _saleRepository = saleRepository;
            _fiscalConfigurationRepository = fiscalConfigurationRepository;
            _naturezaOperacaoRepository = naturezaOperacaoRepository;
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
                    ComapanyId = attempt.CompanyId
                };
            await repository.CreateAsync(entity);
            return new ResponseGeneric { Success=true};
        }

        private async Task<string> TransmitirNfe(NFeEmission nFeEmission, FiscalConfiguration fiscalConfiguration, Sale sale, NaturezaOperacao naturezaOperacao)
        {
            throw new NotImplementedException();
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
    }
}