using Model.Registrations;
using Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service
{
  public class FileService : BaseService<File>, IFileService
   {
      public FileService(IGenericRepository<File> repository) : base(repository)
      {
      }
      public Task<List<File>> GetAllByIdDescriptionFiles(int id)
      {
         return (repository as IFileRepository).GetAllByIdDescriptionFiles(id);
      }

      public Task Delete(File file)
      {
         return (repository as IFileRepository).Delete(file);
      }
   }
   public interface IFileService : IBaseService<File>
   {
      Task<List<File>> GetAllByIdDescriptionFiles(int id);

      Task Delete(File file);
   }
 
}
