using Model.Closure;
using Model.Registrations;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
  public class ClosuresDetailService : BaseService<ClosuresDetail>, IClosuresDetailService
  {
    public ClosuresDetailService(IGenericRepository<ClosuresDetail> repository) : base(repository)
    {
    }

   
  }
  public interface IClosuresDetailService : IBaseService<ClosuresDetail>
  {
  }
}
