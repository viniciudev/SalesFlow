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
      var data = await _dbContext.Set<User>().Where(x => x.Email == model.Email).Include(x=>x.Company)
        .AsNoTracking().SingleOrDefaultAsync();
      return data;
    }
  }

  public interface IUserRepository : IGenericRepository<User>
  {
    Task<User> GetUser(AuthenticateModel model);
  }
}
