using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO;
using Model.Moves;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                              orderby stock.Id descending
                              select new Stock
                              {
                                  Id = stock.Id,
                                  Quantity = stock.Quantity,
                                  Date = stock.Date,
                                  Reason = stock.Reason,
                                  ProductName = stock.Product.Name,
                                  Type = stock.Type,
                                  IdProduct=stock.Product.Id
                              }
                        ).AsNoTracking()
                         .GetPagedAsync<Stock>(filters.PageNumber, filters.PageSize);
            List<StockSummary> stockSummaries = await GetBalance(filters.IdCompany);

            foreach (var item in resp.Results)
            {
            
                decimal ?balance = stockSummaries.FirstOrDefault(x => x.IdProduct == item.IdProduct)?.SaldoAtual;
                if (balance != null)
                {
                    item.Balance = (decimal)balance;
                }
            }
            return resp;
        }
        public async Task<List<StockSummary>> GetBalance(int tenantid)
        {
        var resp = await(from stock in _dbContext.Set<Stock>()
                .Include(x => x.Product)
                         where stock.IdCompany == tenantid
                         group stock by new { stock.IdProduct, stock.Product.Name } into g
                         select new StockSummary
                         {
                             IdProduct = g.Key.IdProduct,
                             ProductName = g.Key.Name,
                             TotalEntradas = g.Where(x => x.Type == StockType.entry).Sum(x => x.Quantity),
                             TotalSaidas = g.Where(x => x.Type == StockType.exit).Sum(x => x.Quantity),
                             SaldoAtual = g.Where(x => x.Type == StockType.entry).Sum(x => x.Quantity) -
                                          g.Where(x => x.Type == StockType.exit).Sum(x => x.Quantity)
                         }
            ).AsNoTracking()
            .ToListAsync();
            return resp;
    }
        public async Task<StockSummary> GetBalanceByProduct(int tenantid,int idProduct)
        {
            var resp = await (from stock in _dbContext.Set<Stock>()
                    .Include(x => x.Product)
                              where stock.IdCompany == tenantid
                              && stock.IdProduct==idProduct
                              select stock
                ).AsNoTracking()
                .ToListAsync();
            

            return  new StockSummary
            {
                //IdProduct = resp.FirstOrDefault()? .IdProduct,
                //ProductName = stock.Name,
                //TotalEntradas = stock.Type == StockType.entry).Sum(x => x.Quantity),
                //TotalSaidas = g.Where(x => x.Type == StockType.exit).Sum(x => x.Quantity),
                SaldoAtual = resp.Where(x => x.Type == StockType.entry).Sum(x => x.Quantity) -
                                               resp.Where(x => x.Type == StockType.exit).Sum(x => x.Quantity)
            };
        }
        public async Task<Stock> GetByReferenceIdAsync(int id)
        {
            var resp = await (from stock in _dbContext.Set<Stock>()
              .Include(x => x.Product)
                              where stock.ReferenceId==id
                              select stock
                              ).AsNoTracking()
          .FirstOrDefaultAsync();
            return resp;
        }
    }
    public interface IStockRepository : IGenericRepository<Stock>
    {
        Task<StockSummary> GetBalanceByProduct(int tenantid, int idProduct);
        Task<PagedResult<Stock>> GetAllPaged(Filters filters);
        Task<Stock> GetByReferenceIdAsync(int id);
    }
}
