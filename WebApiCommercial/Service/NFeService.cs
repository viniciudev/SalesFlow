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


        public NFeService(IGenericRepository<NFeEmission> repository) : base(repository)
        {
            
        }

        public async Task<int> CreateAttemptAsync(NFeEmissionDto attempt)
        {
            attempt.TryCount = attempt.TryCount <= 0 ? 1 : attempt.TryCount;
            attempt.CreatedAt = DateTime.UtcNow;

                var entity = new NFeEmission
                {
                    NaturezaOperacaoId = attempt.NaturezaOperacaoId,
                    SaleId = attempt.SaleId,
                    TipoDocumento = attempt.TipoDocumento,
                    Serie = attempt.Serie,
                    Numero = attempt.Numero,
                    StatusNfe = attempt.StatusNfe,
                    CreatedAt = attempt.CreatedAt,
                    TryCount = attempt.TryCount,
                    ComapanyId = attempt.CompanyId
                };
            await repository.CreateAsync(entity);
            return attempt.Id;
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
        Task<int> CreateAttemptAsync(NFeEmissionDto attempt);
        Task UpdateResultAsync(int id, bool sent, long? numero, string? responseJson, string? errorMessage);
        Task<NFeEmission?> GetByIdAsync(int id);
        Task<List<NFeEmission>> GetPendingAsync();
        Task<List<NFeEmission>> GetBySaleIdAsync(int saleId);
        Task<long?> GetLastNumeroAsync(string serie, TipoDocumentoEnum tipoDocumento);
  
        Task<List<NFeEmission>> GetAll(int tenantid);
        Task<PagedResult<NFeEmission>> GetPaged(Filters filters);
    }
}