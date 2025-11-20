using Model;
using Model.DTO;
using Model.Moves;
using Repository;
using System.Threading.Tasks;

namespace Service
{


    public class StockService : BaseService<Stock>, IStockService
    {

        public StockService(IGenericRepository<Stock> repository
          ) : base(repository)

        {

        }
        public async Task<PagedResult<Stock>> GetAllPaged(Filters filters)
        {
            return await (repository as IStockRepository).GetAllPaged(filters);
        }
        public async Task<StockSummary> GetBalanceByProduct(int tenantid, int idProduct)
        {
            return await (repository as IStockRepository).GetBalanceByProduct(tenantid,idProduct);
        }

    }
    public interface IStockService : IBaseService<Stock>
    {
        Task<StockSummary> GetBalanceByProduct(int tenantid, int idProduct);
        Task<PagedResult<Stock>> GetAllPaged(Filters filters);
    }
}
