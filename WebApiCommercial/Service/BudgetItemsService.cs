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
  public class BudgetItemsService : BaseService<BudgetItems>, IBudgetItemsService
  {
    public BudgetItemsService(IGenericRepository<BudgetItems> repository) : base(repository)
    {
    }
    public Task<PagedResult<BudgetItems>> GeAllByIdBudget(Filters filter)
    {
      return (repository as IBudgetItemsRepository).GeAllByIdBudget(filter);
    }
    public async Task<string> SaveItem(BudgetItems model)
    {
      var value = await (repository as IBudgetItemsRepository).VerifyItemInlist(model);
      if (!value)
      {
        await base.Save(model);
        return "";
      }
      else
        return "Item existente na lista!";
    }
    public Task<List<BudgetItems>> GetListByIdBudget(int idBudget)
    {
      return (repository as IBudgetItemsRepository).GetListByIdBudget(idBudget);
    }
  }
  public interface IBudgetItemsService : IBaseService<BudgetItems>
  {
    Task<PagedResult<BudgetItems>> GeAllByIdBudget(Filters filter);
    Task<string> SaveItem(BudgetItems model);
    Task<List<BudgetItems>> GetListByIdBudget(int idBudget);
  }
  
}
