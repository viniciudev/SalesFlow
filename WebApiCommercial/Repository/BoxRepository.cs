using Microsoft.EntityFrameworkCore;
using Model.Moves;
using System.Threading.Tasks;

namespace Repository
{

    public class BoxRepository : GenericRepository<Box>, IBoxRepository
    {
        public BoxRepository(ContextBase dbContext) : base(dbContext)
        {

        }
        public async Task<Box> GetByStatus(CaixaStatus status)
        {
            return await _dbContext.Set<Box>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Status == status);
                
        }
        public async Task<Box> GetByIdBox(int BoxId)
        {
            return await _dbContext.Set<Box>()
                   .Include(c => c.Movimentacoes)
                   .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == BoxId);
        }
    }
    public interface IBoxRepository : IGenericRepository<Box>
    {
        Task<Box> GetByStatus(CaixaStatus status);
        Task<Box> GetByIdBox(int BoxId);
    }
}
