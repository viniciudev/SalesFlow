using Model.Moves;

namespace Repository
{

    public class FinancialResourceRepository : GenericRepository<FinancialResources>, IFinancialResourceRepository
    {
        public FinancialResourceRepository(ContextBase dbContext) : base(dbContext)
        {

        }

    }
    public interface IFinancialResourceRepository : IGenericRepository<FinancialResources>
    {

    }
}
