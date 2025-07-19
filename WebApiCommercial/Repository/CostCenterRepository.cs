using Microsoft.EntityFrameworkCore;
using Model.Registrations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
  public class CostCenterRepository : GenericRepository<CostCenter>, ICostCenterRepository
  {
    public CostCenterRepository(ContextBase dbContext) : base(dbContext)
    {

    }
    public async Task<List<CostCenter>> GetByIdCompany(int id)
    {
      var data=await _dbContext.Set<CostCenter>().
        Where(x => x.IdCompany == id)
        .AsNoTracking().ToListAsync();
      return data;
    }
  }
  public interface ICostCenterRepository : IGenericRepository<CostCenter>
  {
    Task<List<CostCenter>> GetByIdCompany(int id);
  }
}
