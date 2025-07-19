using Model;
using Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service
{
  public class BaseService<TEntity> : IBaseService<TEntity> where TEntity : class, IEntity, new()
   {
      protected readonly IGenericRepository<TEntity> repository;

      public BaseService(IGenericRepository<TEntity> repository)
      {
         this.repository = repository;
      }

      public async Task Create(TEntity entity)
      {
         await repository.CreateAsync(entity);
      }

      public async Task Alter(TEntity entity)
      {
         await repository.UpdateAsync(entity.Id, entity);
      }

      public async Task<List<TEntity>> GetAll()
      {
         return await repository.GetAll();
      }

      public async Task Save(TEntity entity)
      {
         if (!await repository.Exists(entity.Id))
            await Create(entity);
      }

      public virtual async Task<PagedResult<TEntity>> GetAllPage(int Number, int Size)
      {
         return await repository.GetAllPage(Number, Size);
      }
      public async Task DeleteAsync(int id)
      {
         await repository.DeleteAsync(id);
      }
      public async Task<TEntity> GetByIdAsync(int id)
      {
        return await repository.GetByIdAsync(id);
      }

   }

   public interface IBaseService<TEntity> where TEntity : class, new()
   {
      Task Create(TEntity entity);
      Task Save(TEntity entity);
      Task<List<TEntity>> GetAll();
      Task <TEntity> GetByIdAsync(int id);
      Task Alter(TEntity entity);
      Task<PagedResult<TEntity>> GetAllPage(int Number, int Size);
      Task DeleteAsync(int id);
   }


}
