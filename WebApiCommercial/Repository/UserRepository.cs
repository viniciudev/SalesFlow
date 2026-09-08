using Microsoft.EntityFrameworkCore;
using Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ContextBase dbContext) : base(dbContext)
        {
        }
        public override async Task<List<User>> GetAll()
        {
            return await _dbContext.Set<User>()
                .Where(x => !x.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<User> GetUser(AuthenticateModel model)
        {
            var data = await _dbContext.Set<User>().Where(x => x.Email == model.Email && !x.IsDeleted).Include(x => x.Company)
              .AsNoTracking().SingleOrDefaultAsync();
            return data;
        }
        public async Task<User> GetByToken(string token)
        {
            var data = await _dbContext.Set<User>().Where(x => x.TokenVerify == token && !x.IsDeleted)
    .AsNoTracking().SingleOrDefaultAsync();
            return data;
        }

        public async Task<PagedResult<User>> GetUsersByCompany(Filters filters)
        {
            var data = await _dbContext.Set<User>()
                .Where(x => !x.IsDeleted
                && x.IdCompany == filters.IdCompany
                && ((string.IsNullOrEmpty( filters.TextOption))||( x.Name.Contains(filters.TextOption)))
                )
             .AsNoTracking()
             .GetPagedAsync(filters.PageNumber, filters.PageSize);
            return data;
        }
        public async Task<User> GetUserByEmail(string email)
        {
            var data = await _dbContext.Set<User>()
                .AsNoTracking()
                .Where(x => x.Email == email && !x.IsDeleted)
                .FirstOrDefaultAsync();
            return data;
        }
    }

    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetByToken(string token);
        Task<User> GetUser(AuthenticateModel model);
        Task<User> GetUserByEmail(string email);
        Task<PagedResult<User>> GetUsersByCompany(Filters filters);
    }
}
