using Microsoft.EntityFrameworkCore;
using Model;
using Model.Registrations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
  public class ProviderRepository : GenericRepository<Provider>, IProviderRepository
  {
    public ProviderRepository(ContextBase dbContext) : base(dbContext)
    {
    }

    public async Task<PagedResult<Provider>> GetAllPaged(Filters filters)
    {
      var paged = await base._dbContext.Set<Provider>()
          .Where(x => x.IdCompany == filters.IdCompany
            && (string.IsNullOrEmpty(filters.TextOption)
              || x.nome.Contains(filters.TextOption)
              || x.razaoSocial.Contains(filters.TextOption)
              || x.cnpj.Contains(filters.TextOption)))
          .OrderBy(x => x.nome)
          .Select(p => new Provider
          {
              Id = p.Id,
              nome = p.nome,
              razaoSocial = p.razaoSocial,
              nomeFantasia = p.nomeFantasia,
              cnpj = p.cnpj,
              inscricaoEstadual = p.inscricaoEstadual,
              telefone = p.telefone,
              email = p.email,
              logradouro = p.logradouro,
              numero = p.numero,
              bairro = p.bairro,
              cidade = p.cidade,
              uf = p.uf,
              cep = p.cep,
              complemento = p.complemento,
              IdCompany = p.IdCompany
          })
          .GetPagedAsync<Provider>(filters.PageNumber, filters.PageSize);
      return paged;
    }

    public async Task<List<Provider>> GetListByName(Filters filters)
    {
      var data = await base._dbContext.Set<Provider>().Where(x =>
        x.IdCompany == filters.IdCompany &&
        (string.IsNullOrEmpty(filters.TextOption)
        || x.nome.Contains(filters.TextOption)
        || x.razaoSocial.Contains(filters.TextOption)
        || x.cnpj.Contains(filters.TextOption)))
        .OrderBy(x => x.nome)
        .AsNoTracking().ToListAsync();
      return data;
    }
  }

  public interface IProviderRepository : IGenericRepository<Provider>
  {
    Task<PagedResult<Provider>> GetAllPaged(Filters filters);
    Task<List<Provider>> GetListByName(Filters filters);
  }
}
