using Microsoft.EntityFrameworkCore;
using Model.Moves;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
  public class PhasesProspectsRepository : GenericRepository<PhasesProspects>, IPhasesProspectsRepository
  {
    public PhasesProspectsRepository(ContextBase dbContext) : base(dbContext)
    {
    }
    public async Task<List<PhasesProspects>> GetList(int idProspect)
    {
      var data= await _dbContext.Set<PhasesProspects>()
        .Where(x=>x.IdProspects==idProspect)
        .AsNoTracking().ToListAsync();
      return data;
    }

  }
  public interface IPhasesProspectsRepository : IGenericRepository<PhasesProspects>
  {
    Task<List<PhasesProspects>>GetList(int idProspect);
  }
}
