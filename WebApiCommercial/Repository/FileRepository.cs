using Microsoft.EntityFrameworkCore;
using Model.Registrations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
  public class FileRepository : GenericRepository<File>, IFileRepository
  {
    public FileRepository(ContextBase dbContext) : base(dbContext)
    {
    }
    public async Task<List<File>> GetAllByIdDescriptionFiles(int id)
    {
      var data = await base._dbContext.Set<File>()
        .Where(x => x.IdDescriptionFiles == id)
        .AsNoTracking().ToListAsync();
      return data;
    }

    public async Task Delete(File file)
    {
      _dbContext.Set<File>().Remove(file);
      await _dbContext.SaveChangesAsync();
    }
  }
  public interface IFileRepository : IGenericRepository<File>
  {
    Task<List<File>> GetAllByIdDescriptionFiles(int id);
    Task Delete(File file);
  }

}
