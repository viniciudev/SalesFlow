using Model.Registrations;
using System.Threading.Tasks;

namespace Repository
{
  public class ProviderRepository : GenericRepository<Provider>, IProviderRepository
  {
    public ProviderRepository(ContextBase dbContext) : base(dbContext)
    {
    }

    public async Task<PagedResult<Provider>> GetAllPaged()
    {
      var paged = await base._dbContext.Set<Provider>()

         .GetPagedAsync<Provider>(1, 10);
      return paged;
    }
  }
  public interface IProviderRepository : IGenericRepository<Provider>
  {
    Task<PagedResult<Provider>> GetAllPaged();
  }
}
