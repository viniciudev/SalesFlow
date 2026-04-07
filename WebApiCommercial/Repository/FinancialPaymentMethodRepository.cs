using Model.Moves;

namespace Repository
{

	public class FinancialPaymentMethodRepository : GenericRepository<FinancialPaymentMethod>, IFinancialPaymentMethodRepository
	{
		public FinancialPaymentMethodRepository(ContextBase dbContext) : base(dbContext)
		{
		}
	}
	public interface IFinancialPaymentMethodRepository : IGenericRepository<FinancialPaymentMethod>
	{
	}
}
