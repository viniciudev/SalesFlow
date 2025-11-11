using Model.Moves;
using Repository;

namespace Service
{


    public class StockService : BaseService<Stock>, IStockService
    {
     
        public StockService(IGenericRepository<Stock> repository
          ) : base(repository)

        {
         
        }
        //public Task<Stock> GetStock(AuthenticateModel model)
        //{
        //    return (repository as IStockRepository).GetStock(model);
        //}
    }
    public interface IStockService : IBaseService<Stock>
    {
    }
}
