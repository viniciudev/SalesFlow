using Model;
using Model.DTO;
using Model.Registrations;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
  public class ProspectsService : BaseService<Prospects>, IProspectsService
  {
    public ProspectsService(IGenericRepository<Prospects> repository) : base(repository)
    {
    }
    //public Task<bool> ExistsCpf(string cpf)
    //{
    //   return (repository as IClientRepository).ExistsCpf(cpf);

    //}
    public async Task<PagedResult<Prospects>> GetAllPaged(Filters filters)
    {
      return await (repository as IProspectsRepository).GetAllPaged(filters);
    }
    public async Task<ProspectInfoResponse> GetByMonthAllProspects(Filters filters)
    {
      return await (repository as IProspectsRepository).GetByMonthAllProspects(filters);
    }

  }
  public interface IProspectsService : IBaseService<Prospects>
  {
    Task<PagedResult<Prospects>> GetAllPaged(Filters filters);
    Task<ProspectInfoResponse> GetByMonthAllProspects(Filters filters);
  }
}
