using Microsoft.EntityFrameworkCore;
using Model;
using Model.Registrations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{

    public class PaymentMethodRepository : GenericRepository<PaymentMethod>, IPaymentMethodRepository
    {
        public PaymentMethodRepository(ContextBase dbContext) : base(dbContext)
        {
        }
        public async Task<List<PaymentMethod>> GetAllPage(Filters filter)
        {
            return await _dbContext.Set<PaymentMethod>()
                .Where(x => x.IdCompany == filter.IdCompany)
                .AsNoTracking()
                .ToListAsync();
        }
    }
    public interface IPaymentMethodRepository : IGenericRepository<PaymentMethod>
    {
        Task<List<PaymentMethod>> GetAllPage(Filters filter);
    }
}
