using Model.Registrations;
using Model.Enums;
using Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service
{
    public class NFeService : INFeService
    {
        private readonly INFeRepository _repository;

        public NFeService(INFeRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAttemptAsync(NFeEmission attempt)
        {
            attempt.TryCount = attempt.TryCount <= 0 ? 1 : attempt.TryCount;
            attempt.CreatedAt = DateTime.UtcNow;
            await _repository.CreateAsync(attempt);
            return attempt.Id;
        }

        public async Task UpdateResultAsync(int id, bool sent, long? numero, string? responseJson, string? errorMessage)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) throw new InvalidOperationException("Registro NFe não encontrado.");

            existing.Sent = sent;
            existing.Numero = numero ?? existing.Numero;
            existing.ResponseJson = responseJson;
            existing.ErrorMessage = errorMessage;
            existing.TryCount += 1;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing.Id, existing);
        }

        public async Task<NFeEmission?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<NFeEmission>> GetPendingAsync()
        {
            return await _repository.GetPendingAsync();
        }

        public async Task<List<NFeEmission>> GetBySaleIdAsync(int saleId)
        {
            return await _repository.GetBySaleIdAsync(saleId);
        }

        public async Task<long?> GetLastNumeroAsync(string serie, TipoDocumentoEnum tipoDocumento)
        {
            return await _repository.GetLastNumeroAsync(serie, tipoDocumento);
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
    }
}