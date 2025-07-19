using Microsoft.EntityFrameworkCore;
using Model;
using Model.Moves;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
  public class ServicesProvisionRepository : GenericRepository<ServicesProvision>, IServicesProvisionRepository
  {
    public ServicesProvisionRepository(ContextBase dbContext) : base(dbContext)
    {

    }
    public async Task<PagedResult<ServicesProvision>> GetAllServicesProvisionPaged(Filters filters)
    {
      var data = await (from servicesProvision in base._dbContext.Set<ServicesProvision>()
                        join client in base._dbContext.Set<Client>() 
                        on servicesProvision.IdClient equals client.Id
                        join budget in base._dbContext.Set<Budget>()
                        on servicesProvision.IdBudget equals budget.Id into leftBudget
                        from bud in leftBudget.DefaultIfEmpty()
                        where (
                        (filters.idClient==0|| servicesProvision.IdClient== filters.idClient)
                        && servicesProvision.IdCompany == filters.idCompany)
                        select new ServicesProvision
                        {
                          Id=servicesProvision.Id,
                          IdBudget=servicesProvision.IdBudget,
                          IdClient=servicesProvision.IdClient,
                          IdCompany=servicesProvision.IdCompany,
                          Description=servicesProvision.Description,
                          Date=servicesProvision.Date,
                          NameClient=client.Name,
                          DescriptionBudget= bud.Description,
                        })
        .GetPagedAsync<ServicesProvision>(filters.pageNumber, filters.pageSize);
      return data;
    }

    public async Task<ServicesProvision> GetByIdServiseProvision(int id)
    {
      var data = await (from serviceProvision in _dbContext.Set<ServicesProvision>()
                        .Include(a => a.ServicesProvisionItems)

                        where serviceProvision.Id == id
                        select serviceProvision).AsNoTracking().SingleOrDefaultAsync();
      return data;
    }

  }
  public interface IServicesProvisionRepository : IGenericRepository<ServicesProvision>
  {
    public Task<PagedResult<ServicesProvision>> GetAllServicesProvisionPaged(Filters filters);
    public Task<ServicesProvision> GetByIdServiseProvision(int id);
  }
}
