using Model;
using Model.Moves;
using Model.Registrations;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
  public class PhasesProspectsService : BaseService<PhasesProspects>, IPhasesProspectsService
  {
    public PhasesProspectsService(IGenericRepository<PhasesProspects> repository) : base(repository)
    {
    }
    public async Task<List<PhasesProspects>> GetList(int idProspect)
    {
      return await(repository as IPhasesProspectsRepository).GetList(idProspect);
    }
  }
  public interface IPhasesProspectsService : IBaseService<PhasesProspects>
  {
    Task<List<PhasesProspects>> GetList(int idProspect);
  }
}
