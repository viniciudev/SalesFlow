using Model.Registrations;
using Repository;
using System.Threading.Tasks;

namespace Service
{
    public class CompanyService : BaseService<Company>, ICompanyService
    {
        public CompanyService(IGenericRepository<Company> repository) : base(repository)
        {
        }

        public async Task<Company> GetById(int id)
        {
            var resp=await  ( repository as ICompanyRepository).GetByIdAsync(id);
            return resp;
        }
    }
    public interface ICompanyService : IBaseService<Company>
    {
        Task<Company> GetById(int id);
    }
}
