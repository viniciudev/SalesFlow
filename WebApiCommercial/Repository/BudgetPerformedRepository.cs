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
  public class BudgetPerformedRepository : GenericRepository<BudgetPerformed>, IBudgetPerformedRepository
  {
    public BudgetPerformedRepository(ContextBase dbContext) : base(dbContext)
    {
    }


  }
  public interface IBudgetPerformedRepository : IGenericRepository<BudgetPerformed>
  {
  }
}
