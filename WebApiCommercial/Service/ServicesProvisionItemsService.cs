using Model;
using Model.Moves;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
  public class ServicesProvisionItemsService : BaseService<ServicesProvisionItems>, IServicesProvisionItemsService
  {
    public ServicesProvisionItemsService(IGenericRepository<ServicesProvisionItems> repository) : base(repository)
    {
    }
    public Task<PagedResult<ServicesProvisionItems>> GedByIdServiceProvisionPaged(Filters filter)
    {
      return (repository as IServicesProvisionItemsRepository).GedByIdServiceProvisionPaged(filter);
    }
    public Task<List<ServicesProvisionItems>> GedByIdServiceProvision(Filters filter)
    {
      return (repository as IServicesProvisionItemsRepository).GedByIdServiceProvision(filter);
    }

    public async Task<string> SaveItem(ServicesProvisionItems model)
    {
      var value = await (repository as IServicesProvisionItemsRepository).VerifyItemInlist(model);
      if (!value)
      {
        await base.Save(model);
        return "";
      }
      else
        return "Item existente na lista!";
        
    }

  }
  public interface IServicesProvisionItemsService : IBaseService<ServicesProvisionItems>
  {
   Task<PagedResult<ServicesProvisionItems>> GedByIdServiceProvisionPaged(Filters filter);
    Task<List<ServicesProvisionItems>> GedByIdServiceProvision(Filters filter);
    Task<string> SaveItem(ServicesProvisionItems model);
  }
}
