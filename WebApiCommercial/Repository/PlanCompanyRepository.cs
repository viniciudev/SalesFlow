using Model.Registrations;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Moves;

namespace Repository
{
  public class PlanCompanyRepository : GenericRepository<PlanCompany>, IPlanCompanyRepository
  {
    public PlanCompanyRepository(ContextBase dbContext) : base(dbContext)
    {
    }

   
  }
  public interface IPlanCompanyRepository : IGenericRepository<PlanCompany>
  {
  
  }
}
