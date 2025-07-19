using Model;
using Model.DTO;
using Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service
{
  public class ClientService : BaseService<Client>, IClientService
  {
    public ClientService(IGenericRepository<Client> repository) : base(repository)
    {
    }

    public async Task<PagedResult<Client>> GetAllPaged(Filters clientFilter)
    {
      return await(repository as IClientRepository).GetAllPaged(clientFilter);
    }
    public async Task<List<Client>> GetByName(Filters clientFilter)
    {
      return await (repository as IClientRepository).GetByName(clientFilter);
    }
    public async Task<List<Client>> GetAllList(Filters clientFilter)
    {
      return await (repository as IClientRepository).GetAllList(clientFilter);
    }
    public async Task<ClientInfoResponse> GetByMonthAllClients(Filters filters)
    {
      return await (repository as IClientRepository).GetByMonthAllClients(filters);
    }
  }
  public interface IClientService : IBaseService<Client>
  {
    Task<PagedResult<Client>> GetAllPaged(Filters clientFilter);
    Task<List<Client>> GetByName(Filters clientFilter);
    Task<List<Client>> GetAllList(Filters clientFilter);
    Task<ClientInfoResponse> GetByMonthAllClients(Filters filters);
  }
}
