using Model;
using Model.Registrations;
using Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service
{
   public class ProviderService : BaseService<Provider>, IProviderService
   {
      public ProviderService(IGenericRepository<Provider> repository) : base(repository)
      {
      }

      public Task<PagedResult<Provider>> GetAllPaged(Filters filters)
      {
         return (repository as IProviderRepository).GetAllPaged(filters);
      }

      public Task<List<Provider>> GetListByName(Filters filters)
      {
         return (repository as IProviderRepository).GetListByName(filters);
      }
   }

   public interface IProviderService : IBaseService<Provider>
   {
      Task<PagedResult<Provider>> GetAllPaged(Filters filters);
      Task<List<Provider>> GetListByName(Filters filters);
   }
}
