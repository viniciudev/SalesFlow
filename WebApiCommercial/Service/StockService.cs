using Model;
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
        public Task<PagedResult<Stock>> GetAllPaged(Filters filters)
        {
            return (repository as IStockRepository).GetAllPaged(filters);
        }

    }
    public interface IStockService : IBaseService<Stock>
    {
        Task<PagedResult<Stock>> GetAllPaged(Filters filters);
    }
}
