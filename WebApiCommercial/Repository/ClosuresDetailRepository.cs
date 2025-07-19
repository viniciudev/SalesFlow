using Microsoft.EntityFrameworkCore;
using Model.Closure;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
  public class ClosuresDetailRepository : GenericRepository<ClosuresDetail>, IClosuresDetailRepository
  {
    public ClosuresDetailRepository(ContextBase dbContext) : base(dbContext)
    {

    }
    //public async Task<List<ClosuresDetail>> GetByIdCompany(int id)
    //{
    //  var data = await _dbContext.Set<ClosuresDetail>().
    //    Where(x => x.IdCompany == id)
    //    .AsNoTracking().ToListAsync();
    //  return data;
    //}
  }
  public interface IClosuresDetailRepository : IGenericRepository<ClosuresDetail>
  {
    //Task<List<ClosuresDetail>> GetByIdCompany(int id);
  }
}
