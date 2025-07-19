using Model.Registrations;
using Model;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Moves;

namespace Service
{
  public class PlanCompanyService : BaseService<PlanCompany>, IPlanCompanyService
  {
    public PlanCompanyService(IGenericRepository<PlanCompany> repository) : base(repository)
    {
    }

  }
  public interface IPlanCompanyService : IBaseService<PlanCompany>
  {

  }
}
