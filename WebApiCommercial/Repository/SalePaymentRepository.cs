using Microsoft.EntityFrameworkCore;
using Model.Moves;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
	public class SalePaymentRepository : GenericRepository<SalePayment>, ISalePaymentRepository
	{
		public SalePaymentRepository(ContextBase dbContext) : base(dbContext)
		{
		}

		public async Task<List<SalePayment>> GetBySaleIdAsync(int saleId)
		{
			return await _dbContext.Set<SalePayment>()
				.Include(x => x.PaymentMethod)
				.Where(x => x.IdSale == saleId)
				.AsNoTracking()
				.ToListAsync();
		}

		public async Task DeleteBySaleIdAsync(int saleId)
		{
			var payments = await _dbContext.Set<SalePayment>()
				.Where(x => x.IdSale == saleId)
				.ToListAsync();

			_dbContext.Set<SalePayment>().RemoveRange(payments);
			await _dbContext.SaveChangesAsync();
		}
	}

	public interface ISalePaymentRepository : IGenericRepository<SalePayment>
	{
		Task<List<SalePayment>> GetBySaleIdAsync(int saleId);
		Task DeleteBySaleIdAsync(int saleId);
	}
}
