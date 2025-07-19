using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
  public class ProspectsRepository : GenericRepository<Prospects>, IProspectsRepository
  {
    public ProspectsRepository(ContextBase dbContext) : base(dbContext)
    {
    }

    public async Task<PagedResult<Prospects>> GetAllPaged(Filters filters)
    {
      var paged = await base._dbContext.Set<Prospects>()
        .Where(x=>x.IdCompany==filters.idCompany 
        &&(string.IsNullOrEmpty(filters.textOption)
        ||x.Name.Contains(filters.textOption)))
         .GetPagedAsync<Prospects>(filters.pageNumber, filters.pageSize);
      return paged;
    }
    public async Task<ProspectInfoResponse> GetByMonthAllProspects(Filters filters)
    {
      try
      {
        ProspectInfoResponse clientInfoResponse = new ProspectInfoResponse { AmountMonth = new List<int>(), ProspectAmount = 0 };
        List<Prospects> data = await _dbContext.Set<Prospects>()
          .Where(x => x.IdCompany == filters.idCompany
          && x.RegisterDate.Year == DateTime.Now.Year)
          .AsNoTracking().ToListAsync();

        var grupo1 = data.GroupBy(c => c.RegisterDate.Month)
                                  .Select(g => new { Key = g.Key, Itens = g.ToList() }).ToList();

        for (int i = 1; i < 13; i++)
        {
          var teste = grupo1.FirstOrDefault(x => x.Key == i);
          if (teste != null && teste.Key > 0)
            clientInfoResponse.AmountMonth.Add(teste.Itens.Count);
          else
            clientInfoResponse.AmountMonth.Add(0);
        }
        clientInfoResponse.ProspectAmount = data.Count();
        return clientInfoResponse;
      }
      catch (System.Exception ex)
      {

        throw;
      }
    }
  }
  public interface IProspectsRepository : IGenericRepository<Prospects>
  {
    Task<PagedResult<Prospects>> GetAllPaged(Filters filters);
    Task<ProspectInfoResponse> GetByMonthAllProspects(Filters filters);
  }
}
