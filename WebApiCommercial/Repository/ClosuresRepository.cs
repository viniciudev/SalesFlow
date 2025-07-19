using Microsoft.EntityFrameworkCore;
using Model;
using Model.Closure;
using Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
   public class ClosuresRepository : GenericRepository<Closures>, IClosuresRepository
  {
    public ClosuresRepository(ContextBase dbContext) : base(dbContext)
    {

    }
    public async Task<Closures> GetCheckin(int idSalesman)
    {
      var data = await _dbContext.Set<Closures>().
        Where(x => x.IdSalesman == idSalesman
        && x.Type==Type.CheckIn)
        .AsNoTracking().FirstOrDefaultAsync();
      return data?? new Closures();
    }
		public async Task< PagedResult<ClosuresResponse>> Getpaged(Filters filters)
		{
      try
      {
				var data = await (from closures in _dbContext.Set<Closures>().Include(x => x.ClosuresDetails)
													where (closures.IdSalesman == filters.IdSalesman
													&& filters.CheckinDateFinal.AddHours(23).AddMinutes(59).AddSeconds(59) >= closures.DateInit
													&& filters.CheckinDate <= closures.DateInit)
													select new ClosuresResponse
													{
														Id = closures.Id,
														ClosuresDetails = closures.ClosuresDetails,
														DateFinal = closures.DateFinal,
														DateInit = closures.DateInit,
														kilometerTraveled = closures.OdometerFinal - closures.Odometer,
														ValueSumDetails = closures.ClosuresDetails.Sum(x => x.Value)
													})

				.AsNoTracking().GetPagedAsync(filters.pageNumber, filters.pageSize);
				return data;
			}
      catch (Exception ex)
      {

        throw;
      }
			
     
		}
	}
  public interface IClosuresRepository : IGenericRepository<Closures>
  {
    Task<Closures> GetCheckin(int idSalesman);
    Task<PagedResult<ClosuresResponse>> Getpaged(Filters filters);
	}
}
