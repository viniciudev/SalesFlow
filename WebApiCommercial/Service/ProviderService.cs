using Model.Registrations;
using Repository;
using System.Threading.Tasks;

namespace Service
{
   public class ProviderService : BaseService<Provider>, IProviderService
   {
      public ProviderService(IGenericRepository<Provider> repository) : base(repository)
      {
      }
      //public Task<bool> ExistsCpf(string cpf)
      //{
      //   return (repository as IClientRepository).ExistsCpf(cpf);

      //}
      public Task<PagedResult<Provider>> GetAllPaged()
      {
         return (repository as IProviderRepository).GetAllPaged();
      }

   }
   public interface IProviderService : IBaseService<Provider>
   {
      Task<PagedResult<Provider>> GetAllPaged();
     // Task<bool> ExistsCpf(string cpf);
   }
}
