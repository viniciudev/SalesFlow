using Model.Registrations;

namespace Repository
{

    public class PermissionRepository : GenericRepository<Permission>, IPermissionRepository
    {
        public PermissionRepository(ContextBase dbContext) : base(dbContext)
        {
        }
        //public async Task<List<Permission>> GetAllPage(Filters filter)
        //{
        //    return await _dbContext.Set<Permission>()
        //        .Where(x => x.IdCompany == filter.IdCompany)
        //        .AsNoTracking()
        //        .ToListAsync();
        //}
    }
    public interface IPermissionRepository : IGenericRepository<Permission>
    {
        //Task<List<Permission>> GetAllPage(Filters filter);
    }
}
