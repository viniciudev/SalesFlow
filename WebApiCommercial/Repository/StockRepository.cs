using Model.Moves;

namespace Repository
{

    public class StockRepository : GenericRepository<Stock>, IStockRepository
    {
        public StockRepository(ContextBase dbContext) : base(dbContext)
        {
        }
    }
    public interface IStockRepository : IGenericRepository<Stock>
    {
    }
}
