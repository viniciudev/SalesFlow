using Model.Registrations;
using Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service
{
    public class FiscalConfigurationService : BaseService<FiscalConfiguration>, IFiscalConfigurationService
    {
        private readonly IFiscalConfigurationRepository _fiscalRepository;

        public FiscalConfigurationService(IGenericRepository<FiscalConfiguration> repository,
                                          IFiscalConfigurationRepository fiscalRepository) : base(repository)
        {
            _fiscalRepository = fiscalRepository;
        }

        public async Task<FiscalConfiguration?> GetActiveAsync(int tenantid)
        {
            return await _fiscalRepository.GetActiveAsync(tenantid);
        }

        // Herdamos Create, Alter, GetAll, GetByIdAsync, DeleteAsync do BaseService/IGenericRepository.
        // Se quiser lógica adicional (validações) adicione métodos aqui.
    }
    public interface IFiscalConfigurationService : IBaseService<FiscalConfiguration>
    {
        Task<FiscalConfiguration?> GetActiveAsync(int tenantid);
    }
}