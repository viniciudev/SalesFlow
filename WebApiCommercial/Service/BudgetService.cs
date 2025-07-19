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
  public class BudgetService : BaseService<Budget>, IBudgetService
  {
    public BudgetService(IGenericRepository<Budget> repository) : base(repository)
    {
    }
    public Task<PagedResult<Budget>> GetAllPaged(Filters filters)
    {
      return (base.repository as IBudgetRepository).GetAllPaged(filters);
    }
    public Task<List<Budget>> GetByDescription(Filters filters)
    {
      return (repository as IBudgetRepository).GetByDescription(filters);
    }
    public Task<Budget> GetByIdBudget(int id)
    {
      return (repository as IBudgetRepository).GetByIdBudget(id);
    }
  }
  public interface IBudgetService : IBaseService<Budget>
  {
    Task<PagedResult<Budget>> GetAllPaged(Filters filters);
    Task<List<Budget>> GetByDescription(Filters filters);
    public Task<Budget> GetByIdBudget(int id);
  }
}
