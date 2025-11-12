using Microsoft.EntityFrameworkCore;
using Model;
using Model.Moves;
using System.Threading.Tasks;
using System.Linq;
namespace Repository
{

    public class StockRepository : GenericRepository<Stock>, IStockRepository
    {
        public StockRepository(ContextBase dbContext) : base(dbContext)
        {
        }
        public async Task<PagedResult<Stock>> GetAllPaged(Filters filters)
        {

            var resp = await (from stock in _dbContext.Set<Stock>()
                        .Include(x => x.Product)
                              where stock.IdCompany == filters.IdCompany
                              select new Stock
                              {
                                  Id = stock.Id,
                                  Quantity = stock.Quantity,
                                  Date = stock.Date,
                                  Reason = stock.Reason,
                                  ProductName = stock.Product.Name,
                                  Type = stock.Type,
                              }
                        ).AsNoTracking()
                         .GetPagedAsync<Stock>(filters.PageNumber, filters.PageSize);
            return resp;
        }
        public async Task<dynamic> GetBalance(int tenantid)
        {
            var resp = await (from stock in _dbContext.Set<Stock>()
                      .Include(x => x.Product)
                              where stock.IdCompany == tenantid
                              select new Stock
                              {
                                  Id = stock.Id,
                                  Quantity = stock.Quantity,
                                  Date = stock.Date,
                                  Reason = stock.Reason,
                                  ProductName = stock.Product.Name,
                                  Type = stock.Type,
                              }
                      ).AsNoTracking()
                      .ToListAsync();
         
        } 
    }
    public interface IStockRepository : IGenericRepository<Stock>
    {
        Task<PagedResult<Stock>> GetAllPaged(Filters filters);
    }
}
