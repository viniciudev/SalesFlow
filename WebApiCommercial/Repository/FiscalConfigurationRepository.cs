using Microsoft.EntityFrameworkCore;
using Model.Registrations;
using System.Threading.Tasks;

namespace Repository
{
    public class FiscalConfigurationRepository : GenericRepository<FiscalConfiguration>, IFiscalConfigurationRepository
    {
        public FiscalConfigurationRepository(ContextBase dbContext) : base(dbContext)
        {
        }

        public async Task<FiscalConfiguration?> GetActiveAsync(int tenantid)
        {
            // Implementação simples: retorna a primeira configuração (você pode ter flag IsActive ou CompanyId)
            return await _dbContext.Set<FiscalConfiguration>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x=>x.CompanyId==tenantid);
        }
        public async Task<FiscalConfiguration?> GetByCompany(int tenantid)
        {
            return await _dbContext.Set<FiscalConfiguration>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CompanyId == tenantid);
        }
    }
    public interface IFiscalConfigurationRepository : IGenericRepository<FiscalConfiguration>
    {
        Task<FiscalConfiguration?> GetActiveAsync(int tenantid); // exemplo: retorna configuração ativa (você pode adaptar)
        Task<FiscalConfiguration?> GetByCompany(int companyId);
    }
}