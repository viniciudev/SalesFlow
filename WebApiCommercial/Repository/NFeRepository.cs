using Microsoft.EntityFrameworkCore;
using Model.Registrations;
using Model.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    public class NFeRepository : GenericRepository<NFeEmission>, INFeRepository
    {
        public NFeRepository(ContextBase dbContext) : base(dbContext)
        {
        }

        public async Task<List<NFeEmission>> GetPendingAsync()
        {
            return await _dbContext.Set<NFeEmission>()
                .Where(x => !x.Sent)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<NFeEmission?> GetByIdAsync(int id)
        {
            return await _dbContext.Set<NFeEmission>()
                .AsNoTracking()
                .Include(x => x.Company)
                .Include(x => x.NaturezaOperacao)
                .Include(x => x.Sale)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<NFeEmission>> GetBySaleIdAsync(int saleId)
        {
            return await _dbContext.Set<NFeEmission>()
                .Where(x => x.SaleId == saleId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<long?> GetLastNumeroAsync(string serie, TipoDocumentoEnum tipoDocumento)
        {
            var last = await _dbContext.Set<NFeEmission>()
                .Where(x => x.Serie == serie && x.TipoDocumento == tipoDocumento && x.Numero.HasValue)
                .OrderByDescending(x => x.Numero)
                .Select(x => x.Numero)
                .FirstOrDefaultAsync();

            return last;
        }
    }
 
        public interface INFeRepository : IGenericRepository<NFeEmission>
        {
            Task<List<NFeEmission>> GetPendingAsync();
            Task<NFeEmission?> GetByIdAsync(int id);
            Task<List<NFeEmission>> GetBySaleIdAsync(int saleId);
            Task<long?> GetLastNumeroAsync(string serie, TipoDocumentoEnum tipoDocumento);
        }
    
}