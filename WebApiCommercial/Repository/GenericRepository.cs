using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
  public class GenericRepository<TEntity> : IGenericRepository<TEntity>
      where TEntity : class, IEntity, new()
  {
    public DbContext _dbContext { get; set; }
    public GenericRepository(ContextBase dbContext)
    {
      _dbContext = dbContext;
    }

    public async Task CreateAsync(TEntity entity)
    {
      await _dbContext.Set<TEntity>().AddAsync(entity);
      await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> Exists(int id)
    {
      return await _dbContext.Set<TEntity>().AnyAsync(x => x.Id == id);
    }

    public async Task DeleteAsync(int id)
    {
      var entity = await GetByIdAsync(id);
      _dbContext.Set<TEntity>().Remove(entity);
      await _dbContext.SaveChangesAsync();
    }

    public virtual async Task<List<TEntity>> GetAll()
    {
      return await _dbContext.Set<TEntity>().AsNoTracking().ToListAsync();
    }

    public virtual async Task<PagedResult<TEntity>> GetAllPage(int PageNumber, int PageSize)
    {

      var paged = await _dbContext.Set<TEntity>().GetPagedAsync<TEntity>(PageNumber, PageSize);
      return paged;
    }

    public async Task<TEntity> GetByIdAsync(int id)
    {
      return await _dbContext.Set<TEntity>()
                     .AsNoTracking()
                     .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task UpdateAsync(int id, TEntity entity)
    {
      _dbContext.Set<TEntity>().Update(entity);
      await _dbContext.SaveChangesAsync();
    }
    public async Task<IDbContextTransaction> CreateTransactionAsync()
    {
      return await _dbContext.Database.BeginTransactionAsync();
    }
  }

  public interface IGenericRepository<TEntity> where TEntity : class, IEntity, new()
  {
    Task<List<TEntity>> GetAll();
    Task<PagedResult<TEntity>> GetAllPage(int PageNumber, int PageSize);
    Task<TEntity> GetByIdAsync(int id);
    Task CreateAsync(TEntity entity);
    Task UpdateAsync(int id, TEntity entity);
    Task DeleteAsync(int id);
    Task<bool> Exists(int id);
    Task<IDbContextTransaction> CreateTransactionAsync();
  }
}
