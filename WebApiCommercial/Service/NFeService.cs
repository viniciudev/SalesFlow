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

        public async Task<int> CreateAttemptAsync(NFeEmission attempt)
        {
            attempt.TryCount = attempt.TryCount <= 0 ? 1 : attempt.TryCount;
            attempt.CreatedAt = DateTime.UtcNow;
            await repository.CreateAsync(attempt);
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
    }
    public interface INFeService
    {
        Task<int> CreateAttemptAsync(NFeEmission attempt);
        Task UpdateResultAsync(int id, bool sent, long? numero, string? responseJson, string? errorMessage);
        Task<NFeEmission?> GetByIdAsync(int id);
        Task<List<NFeEmission>> GetPendingAsync();
        Task<List<NFeEmission>> GetBySaleIdAsync(int saleId);
        Task<long?> GetLastNumeroAsync(string serie, TipoDocumentoEnum tipoDocumento);
  
        Task<List<NFeEmission>> GetAll(int tenantid);
    }
}