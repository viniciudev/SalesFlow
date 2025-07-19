using Model;
using Model.Registrations;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
  public class ServiceProvidedService : BaseService<ServiceProvided>, IServiceProvidedService
  {
    public ServiceProvidedService(IGenericRepository<ServiceProvided> repository) : base(repository)
    {
    }

    public async Task<PagedResult<ServiceProvided>> GetAllPaged(Filters filter)
    {
      return await (repository as IServiceProvidedRepository).GetAllPaged(filter);
    }
    public async Task<List<ServiceProvided>> GetListByName(Filters filter)
    {
      return await (repository as IServiceProvidedRepository).GetListByName(filter);
    }

  }
  public interface IServiceProvidedService : IBaseService<ServiceProvided>
  {
    Task<PagedResult<ServiceProvided>> GetAllPaged(Filters filter);
    Task<List<ServiceProvided>> GetListByName(Filters filter);
  }
}
