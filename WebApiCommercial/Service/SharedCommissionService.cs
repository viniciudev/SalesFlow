using Model.Moves;
using Model;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
  public class SharedCommissionService : BaseService<SharedCommission>, ISharedCommissionService
  {
  
    public SharedCommissionService(IGenericRepository<SharedCommission> repository
    ) : base(repository)
    {

    }
    public async Task<SharedCommission> GetByIdSaleItems(int id)
    {
      return await(repository as ISharedCommissionRepository).GetByIdSaleItems(id);
    }
  }
  public interface ISharedCommissionService : IBaseService<SharedCommission>
  {
    Task<SharedCommission> GetByIdSaleItems(int id);
  }
}
