using Microsoft.EntityFrameworkCore;
using Model.Enums;
using Model.Registrations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{

    public class UserPermissionRepository : GenericRepository<UserPermission>, IUserPermissionRepository
    {
        public UserPermissionRepository(ContextBase dbContext) : base(dbContext)
        {
        }
        //public async Task<List<UserPermission>> GetAllPage(Filters filter)
        //{
        //    return await _dbContext.Set<UserPermission>()
        //        .Where(x => x.IdCompany == filter.IdCompany)
        //        .AsNoTracking()
        //        .ToListAsync();
        //}
        public async Task<bool> UserPermissions(string userId, PermissionEnum permissionCode)
        {
            
           return await _dbContext.Set<UserPermission>()
                .AnyAsync(up => up.UserId == int.Parse(userId) &&
               up.Permission.Code == permissionCode);
        }
        public async Task<List<UserPermission>> UserPermissions(int userId)
        {
            return await _dbContext.Set<UserPermission>()
                .Where(up => up.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }
    }
    public interface IUserPermissionRepository : IGenericRepository<UserPermission>
    {
        //Task<List<UserPermission>> GetAllPage(Filters filter);
        Task<bool> UserPermissions(string userId, PermissionEnum permissionCode);
        Task<List<UserPermission>> UserPermissions(int userId);
    }
}
