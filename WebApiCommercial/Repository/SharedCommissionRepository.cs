using Microsoft.EntityFrameworkCore;
using Model.Moves;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
  public class SharedCommissionRepository : GenericRepository<SharedCommission>, ISharedCommissionRepository
  {
    public SharedCommissionRepository(ContextBase dbContext) : base(dbContext)
    {

    }
    public async Task<SharedCommission> GetByIdSaleItems(int id)
    {
      var data = await _dbContext.Set<SharedCommission>()
        .Where(x => x.IdSaleItems == id).AsNoTracking().FirstOrDefaultAsync();
      return data;
    }
  }
  public interface ISharedCommissionRepository : IGenericRepository<SharedCommission>
  {
    Task<SharedCommission> GetByIdSaleItems(int id);
  }
 
}
