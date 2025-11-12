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
            public class StockResumo
        {
            public int IdProduct { get; set; }
            public string ProductName { get; set; }
            public decimal TotalEntradas { get; set; }
            public decimal TotalSaidas { get; set; }
            public decimal SaldoAtual { get; set; }
        }

        var resp = await(from stock in _dbContext.Set<Stock>()
                .Include(x => x.Product)
                         where stock.IdCompany == tenantid
                         group stock by new { stock.IdProduct, stock.Product.Name } into g
                         select new StockResumo
                         {
                             IdProduct = g.Key.IdProduct,
                             ProductName = g.Key.Name,
                             TotalEntradas = g.Where(x => x.Type == 1).Sum(x => x.Quantity),
                             TotalSaidas = g.Where(x => x.Type == 2).Sum(x => x.Quantity),
                             SaldoAtual = g.Where(x => x.Type == 1).Sum(x => x.Quantity) -
                                          g.Where(x => x.Type == 2).Sum(x => x.Quantity)
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
