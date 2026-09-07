using Model.Moves;

namespace Repository
{
    public class PurchaseItemRepository : GenericRepository<PurchaseItem>, IPurchaseItemRepository
    {
        public PurchaseItemRepository(ContextBase dbContext) : base(dbContext)
        {
        }
        
    }

    public interface IPurchaseItemRepository : IGenericRepository<PurchaseItem>
    {
        
    }
}