using Model.Registrations;
using Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service
{
  public class CostCenterService : BaseService<CostCenter>, ICostCenterService
  {
    public CostCenterService(IGenericRepository<CostCenter> repository) : base(repository)
    {
    }

    public async Task<List<CostCenter>> GetByIdCompany(int id)
    {
      return await(repository as ICostCenterRepository).GetByIdCompany(id);
    }
  }
  public interface ICostCenterService : IBaseService<CostCenter>
  {
    Task<List<CostCenter>> GetByIdCompany(int id);
  }
}
