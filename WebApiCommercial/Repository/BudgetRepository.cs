using Microsoft.EntityFrameworkCore;
using Model;
using Model.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
  public class BudgetRepository : GenericRepository<Budget>, IBudgetRepository
  {
    public BudgetRepository(ContextBase dbContext) : base(dbContext)
    {

    }
    public async Task<PagedResult<Budget>> GetAllPaged(Filters filters)
    {
      var data= await (from budget in base._dbContext.Set<Budget>()
                      join client in base._dbContext.Set<Client>()on budget.IdClient equals client.Id
                      where (budget.IdCompany==filters.idCompany) &&
                      (filters.idClient==0 || budget.IdClient==filters.idClient)
                      select new Budget
                      {
                        Id=budget.Id,
                        Description=budget.Description,
                        IdClient=budget.IdClient,
                        IdCompany=budget.IdCompany,
                        NameClient=client.Name,
                        Date=budget.Date
                      })
        .GetPagedAsync<Budget>(filters.pageNumber, filters.pageSize);
      return data;
    }
    public async Task<List<Budget>> GetByDescription(Filters filters)
    {
      var data = await _dbContext.Set<Budget>().
        Where(x => x.Description.Contains(filters.textOption)).AsNoTracking().ToListAsync();
      return data;
    }

    public async Task<Budget> GetByIdBudget(int id)
    {
      var data = await (from budget in _dbContext.Set<Budget>().Include(a=>a.BudgetItems)
                                        where budget.Id == id
                                        select budget
                       ).AsNoTracking().SingleOrDefaultAsync();
      return data;
    }

  }
  public interface IBudgetRepository : IGenericRepository<Budget>
  {
    Task<PagedResult<Budget>> GetAllPaged(Filters filters);
    Task<Budget> GetByIdBudget(int id);
    Task<List<Budget>> GetByDescription(Filters filters);
  }
}
