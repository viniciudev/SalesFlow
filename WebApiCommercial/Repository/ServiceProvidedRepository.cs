using Microsoft.EntityFrameworkCore;
using Model;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
  public class ServiceProvidedRepository : GenericRepository<ServiceProvided>, IServiceProvidedRepository
  {
    public ServiceProvidedRepository(ContextBase dbContext) : base(dbContext)
    {
    }

    public async Task<PagedResult<ServiceProvided>> GetAllPaged(Filters filter)
    {
      var paged = await base._dbContext.Set<ServiceProvided>().Where(x => x.IdCompany == filter.idCompany).
        GetPagedAsync<ServiceProvided>(filter.pageNumber, filter.pageSize);
      return paged;
    }
    public async Task<List<ServiceProvided>> GetListByName(Filters filters)
    {
      var data = await base._dbContext.Set<ServiceProvided>().Where(x =>
      x.IdCompany == filters.idCompany &&
      (string.IsNullOrEmpty(filters.textOption) || x.Name.Contains(filters.textOption)))
        .AsNoTracking().ToListAsync();
      return data;
    }
  }
  public interface IServiceProvidedRepository : IGenericRepository<ServiceProvided>
  {
    Task<PagedResult<ServiceProvided>> GetAllPaged(Filters filter);
    Task<List<ServiceProvided>> GetListByName(Filters filters);
  }
}
