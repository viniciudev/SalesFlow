using Microsoft.EntityFrameworkCore;
using Model;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ContextBase dbContext) : base(dbContext)
        {
        }
        public async Task<User> GetUser(AuthenticateModel model)
        {
            var data = await _dbContext.Set<User>().Where(x => x.Email == model.Email).Include(x => x.Company)
              .AsNoTracking().SingleOrDefaultAsync();
            return data;
        }
        public async Task<User> GetByToken(string token)
        {
            var data = await _dbContext.Set<User>().Where(x => x.TokenVerify == token)
    .AsNoTracking().SingleOrDefaultAsync();
            return data;
        }

        public async Task<PagedResult<User>> GetUsersByCompany(Filters filters)
        {
            var data = await _dbContext.Set<User>()
                .Where(x => x.IdCompany == filters.IdCompany 
                && ((string.IsNullOrEmpty( filters.TextOption))||( x.Name.Contains(filters.TextOption)))
                )
             .AsNoTracking()
             .GetPagedAsync(filters.PageNumber, filters.PageSize);
            return data;
        }
    }

    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetByToken(string token);
        Task<User> GetUser(AuthenticateModel model);
        Task<PagedResult<User>> GetUsersByCompany(Filters filters);
    }
}
