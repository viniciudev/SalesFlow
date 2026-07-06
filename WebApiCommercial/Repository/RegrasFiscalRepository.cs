using Microsoft.EntityFrameworkCore;
using Model.Registrations;
using System.Threading.Tasks;

namespace Repository
{

	public class RegrasFiscalRepository : GenericRepository<RegraFiscal>, IRegrasFiscalRepository
	{
		public RegrasFiscalRepository(ContextBase dbContext) : base(dbContext)
		{
		}
		public async Task<RegraFiscal> GetByIdNaturezaAsync(int id)
		{
			return await _dbContext.Set<RegraFiscal>()
				//.Include(r => r.NaturezaOperacao)
				//.Include(r => r.SituacaoTributaria)
				.AsNoTracking()
				.FirstOrDefaultAsync(r => r.NaturezaOperacaoId == id);
		}
	}

	public interface IRegrasFiscalRepository : IGenericRepository<RegraFiscal>
	{
		Task<RegraFiscal> GetByIdNaturezaAsync(int id);
	}
}
