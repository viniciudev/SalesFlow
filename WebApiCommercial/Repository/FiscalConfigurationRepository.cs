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

        public async Task<FiscalConfiguration?> GetActiveAsync()
        {
            // Implementação simples: retorna a primeira configuração (você pode ter flag IsActive ou CompanyId)
            return await _dbContext.Set<FiscalConfiguration>()
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        // GetAll, GetById, Create, Update, Delete já vêm do GenericRepository
    }
    public interface IFiscalConfigurationRepository : IGenericRepository<FiscalConfiguration>
    {
        Task<FiscalConfiguration?> GetActiveAsync(); // exemplo: retorna configuração ativa (você pode adaptar)
    }
}