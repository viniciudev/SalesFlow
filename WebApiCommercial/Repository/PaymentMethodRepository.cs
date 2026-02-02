using Model.Registrations;

namespace Repository
{

    public class PaymentMethodRepository : GenericRepository<PaymentMethod>, IPaymentMethodRepository
    {
        public PaymentMethodRepository(ContextBase dbContext) : base(dbContext)
        {
        }


    }
    public interface IPaymentMethodRepository : IGenericRepository<PaymentMethod>
    {

    }
}
