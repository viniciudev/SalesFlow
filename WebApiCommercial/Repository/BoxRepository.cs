using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO.BoxDto;
using Model.Moves;
using System.Linq;
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
        public async Task<PagedResult<Box>> GetPaged(Filters filters)
        {
            return await _dbContext.Set<Box>()
                .Where(x => x.IdCompany == filters.IdCompany)
                .AsNoTracking()
                .GetPagedAsync(filters.PageNumber, filters.PageSize);
        }
        public async Task<BoxStatus> GetStatusByCompany(int tenantid)
        {
            var data= await (from b in _dbContext.Set<Box>()
                          where b.IdCompany == tenantid
                          orderby b.Id descending
                          select b).FirstOrDefaultAsync();

            return new BoxStatus { CaixaAberto = data };
        }
    }
    public interface IBoxRepository : IGenericRepository<Box>
    {
        Task<Box> GetByStatus(CaixaStatus status);
        Task<Box> GetByIdBox(int BoxId);
        Task<PagedResult<Box>> GetPaged(Filters filters);
        Task<BoxStatus> GetStatusByCompany(int tenantid);
    }
}
