using Model.Registrations;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace Repository
{
  public class SalesmanRepository : GenericRepository<Salesman>, ISalesmanRepository
  {
    public SalesmanRepository(ContextBase dbContext) : base(dbContext)
    {
    }

    public async Task<PagedResult<Salesman>> GetAllPaged(Filters filter)
    {
      var paged = await base._dbContext.Set<Salesman>().Where(x => x.IdCompany == filter.idCompany).
        GetPagedAsync<Salesman>(filter.pageNumber, filter.pageSize);
      return paged;
    }
    public async Task<List<Salesman>> GetListByName(Filters filters)
    {
      var data = await base._dbContext.Set<Salesman>().Where(x =>
      x.IdCompany==filters.idCompany &&
      x.Name.Contains(filters.textOption)).AsNoTracking().ToListAsync();
      return data;
    }
    public async Task<List<Salesman>> GetAllList(Filters filters)
    {
      var data = await(from  salesman in base._dbContext.Set<Salesman>() 
                       where salesman.IdCompany == filters.idCompany
                       select new Salesman
                       {
                         Id=salesman.Id,
                         Name=salesman.Name,
                       }
                       ).AsNoTracking().ToListAsync();
      return data;
    }
    public async Task<List<Salesman>> GetAllByGuid(string guid)
    {
      var data = await (from salesman in base._dbContext.Set<Salesman>()
                        join company in base._dbContext.Set<Company>()
                        on salesman.IdCompany equals company.Id
                        where company.Guid == Guid.Parse(guid)
                        select new Salesman
                        {
                          Id = salesman.Id,
                          Name = salesman.Name,
                        }
                     ).AsNoTracking().ToListAsync();
      return data;
    }
  }
  public interface ISalesmanRepository : IGenericRepository<Salesman>
  {
    Task<PagedResult<Salesman>> GetAllPaged(Filters filter);
    Task<List<Salesman>> GetListByName(Filters filters);
    Task<List<Salesman>> GetAllList(Filters filters);
    Task<List<Salesman>> GetAllByGuid (string guid);
  }
}
