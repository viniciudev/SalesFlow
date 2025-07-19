using Microsoft.EntityFrameworkCore;
using Model;
using Model.Moves;
using Model.Registrations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
  public class BudgetItemsRepository : GenericRepository<BudgetItems>, IBudgetItemsRepository
  {
    public BudgetItemsRepository(ContextBase dbContext) : base(dbContext)
    {

    }

    public async Task<PagedResult<BudgetItems>> GeAllByIdBudget(Filters filter)
    {
      var data = await (from budgetItems in base._dbContext.Set<BudgetItems>()


                        join product in base._dbContext.Set<Product>() on budgetItems.IdItem equals product.Id
                        into prodLeft
                        from prod in prodLeft.DefaultIfEmpty()

                        join service in base._dbContext.Set<ServiceProvided>() on budgetItems.IdItem equals service.Id
                        into serLeft
                        from ser in serLeft.DefaultIfEmpty()

                        where (budgetItems.IdBudget == filter.idBudget)
                        select new BudgetItems
                        {
                          Id = budgetItems.Id,
                          TypeItem = budgetItems.TypeItem,
                          NameItem = budgetItems.TypeItem == TypeItem.Product ? prod.Name : ser.Name,
                          Value = budgetItems.Value,
                          Amount = budgetItems.Amount,
                          Date = budgetItems.Date,
                          IdBudget = budgetItems.IdBudget,
                          IdItem = budgetItems.IdItem
                        }
                       )
        .GetPagedAsync<BudgetItems>(filter.pageNumber, filter.pageSize);
      return data;
    }
    public async Task<bool> VerifyItemInlist(BudgetItems model)
    {
      var data = await base._dbContext.Set<BudgetItems>().Where(x =>
      x.IdBudget == model.IdBudget
      && x.IdItem == model.IdItem
      && x.TypeItem == model.TypeItem).AsNoTracking().AnyAsync();
      return data;
    }
    public async Task<List<BudgetItems>> GetListByIdBudget(int idBudget)
    {
      var data = await (from budgetItems in base._dbContext.Set<BudgetItems>()


                        join product in base._dbContext.Set<Product>() on budgetItems.IdItem equals product.Id
                        into prodLeft
                        from prod in prodLeft.DefaultIfEmpty()

                        join service in base._dbContext.Set<ServiceProvided>() on budgetItems.IdItem equals service.Id
                        into serLeft
                        from ser in serLeft.DefaultIfEmpty()

                        where (budgetItems.IdBudget == idBudget)
                        select budgetItems)
        .AsNoTracking().ToListAsync();
      return data;
    }

  }
  public interface IBudgetItemsRepository : IGenericRepository<BudgetItems>
  {
    Task<PagedResult<BudgetItems>> GeAllByIdBudget(Filters filter);
    Task<bool> VerifyItemInlist(BudgetItems model);
    Task<List<BudgetItems>> GetListByIdBudget(int idBudget);
  }

}


