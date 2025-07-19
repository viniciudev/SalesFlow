using Microsoft.EntityFrameworkCore;
using Model;
using Model.Moves;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
  public class ServicesProvisionItemsRepository : GenericRepository<ServicesProvisionItems>, IServicesProvisionItemsRepository
  {
    public ServicesProvisionItemsRepository(ContextBase dbContext) : base(dbContext)
    {

    }

    public async Task<PagedResult<ServicesProvisionItems>> GedByIdServiceProvisionPaged(Filters filter)
    {
      var data = await (from ServicesProvisionItems in base._dbContext.Set<ServicesProvisionItems>()

                        join product in base._dbContext.Set<Product>() on ServicesProvisionItems.IdItem equals product.Id into productLeft
                        from prod in productLeft.DefaultIfEmpty()
                        join service in base._dbContext.Set<ServiceProvided>() on ServicesProvisionItems.IdItem equals service.Id into serviceLeft
                        from ser in serviceLeft.DefaultIfEmpty()

                        where (ServicesProvisionItems.IdServiceProvision == filter.idServiceProvision)
                        select new ServicesProvisionItems
                        {
                          Id = ServicesProvisionItems.Id,
                          TypeItem = ServicesProvisionItems.TypeItem,
                          NameItem = ServicesProvisionItems.TypeItem == TypeItemServices.Product ? prod.Name : ser.Name,
                          Value = ServicesProvisionItems.Value,
                          Amount = ServicesProvisionItems.Amount,
                          Date = ServicesProvisionItems.Date,
                          IdItem= ServicesProvisionItems.IdItem,
                          IdServiceProvision= ServicesProvisionItems.IdServiceProvision
                        }
                       )
        .GetPagedAsync<ServicesProvisionItems>(filter.pageNumber, filter.pageSize);
      return data;
    }

    public async Task<List<ServicesProvisionItems>> GedByIdServiceProvision(Filters filter)
    {
      var data = await (from ServicesProvisionItems in base._dbContext.Set<ServicesProvisionItems>()

                        join product in base._dbContext.Set<Product>() on ServicesProvisionItems.IdItem equals product.Id
                        into prodLeft from prod in prodLeft.DefaultIfEmpty()
                        join service in base._dbContext.Set<ServiceProvided>() on ServicesProvisionItems.IdItem equals service.Id
                        into serLeft from ser in serLeft.DefaultIfEmpty()
                        where (ServicesProvisionItems.IdServiceProvision == filter.idServiceProvision)
                        select new ServicesProvisionItems
                        {
                          Id = ServicesProvisionItems.Id,
                          TypeItem = ServicesProvisionItems.TypeItem,
                          NameItem = ServicesProvisionItems.TypeItem == TypeItemServices.Product ? prod.Name : ser.Name,
                          Value = ServicesProvisionItems.Value,
                          Amount = ServicesProvisionItems.Amount,
                          Date = ServicesProvisionItems.Date,
                          IdItem=ServicesProvisionItems.IdItem                        }
                       ).AsNoTracking().ToListAsync();
      return data;
    }
    public  async Task<bool> VerifyItemInlist(ServicesProvisionItems model)
    {
      var data = await base._dbContext.Set<ServicesProvisionItems>().Where(x =>
      x.IdServiceProvision == model.IdServiceProvision
      && x.IdItem == model.IdItem
      && x.TypeItem == model.TypeItem).AsNoTracking().AnyAsync();
      return data;
    }

  }
  public interface IServicesProvisionItemsRepository : IGenericRepository<ServicesProvisionItems>
  {
    Task<PagedResult<ServicesProvisionItems>> GedByIdServiceProvisionPaged(Filters filter);
    Task<List<ServicesProvisionItems>> GedByIdServiceProvision(Filters filter);
    Task<bool> VerifyItemInlist(ServicesProvisionItems model);
  }
}
