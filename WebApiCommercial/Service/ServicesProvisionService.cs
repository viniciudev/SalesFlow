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
  public class ServicesProvisionService : BaseService<ServicesProvision>, IServicesProvisionService
  {
    private IBudgetItemsService budgetItemsService;
    private IServicesProvisionItemsService servicesProvisionItemsService;
    public ServicesProvisionService(IGenericRepository<ServicesProvision> repository,
      IBudgetItemsService budgetItemsService,
      IServicesProvisionItemsService servicesProvisionItemsService) : base(repository)
    {
      this.budgetItemsService = budgetItemsService;
      this.servicesProvisionItemsService = servicesProvisionItemsService;
    }
    public Task<PagedResult<ServicesProvision>> GetAllServicesProvisionPaged(Filters filters)
    {
      return (repository as IServicesProvisionRepository).GetAllServicesProvisionPaged(filters);
    }
    public Task<ServicesProvision> GetByIdServiseProvision(int id)
    {
      return (repository as IServicesProvisionRepository).GetByIdServiseProvision(id);
    }
    public async Task SaveService(ServicesProvision model)
    {
      await base.Save(model);
      if (model.IdBudget > 0)
      {

        List<BudgetItems> budgetItems = await budgetItemsService.GetListByIdBudget((int)model.IdBudget);
        if (budgetItems.Count > 0)
        {
          foreach (var item in budgetItems)
          {
            ServicesProvisionItems itemsService = new ServicesProvisionItems();
            itemsService.Amount = item.Amount;
            itemsService.Date = item.Date;
            itemsService.IdItem = item.IdItem;
            itemsService.IdServiceProvision = model.Id;
            itemsService.TypeItem = (TypeItemServices)item.TypeItem;
            itemsService.Value = item.Value;

            await servicesProvisionItemsService.Save(itemsService);
          }
          
        }
      }
    }

  }
  public interface IServicesProvisionService : IBaseService<ServicesProvision>
  {
    Task<ServicesProvision> GetByIdServiseProvision(int id);
    Task<PagedResult<ServicesProvision>> GetAllServicesProvisionPaged(Filters filters);
    Task SaveService(ServicesProvision model);
  }
}
