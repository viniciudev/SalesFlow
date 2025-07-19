using Microsoft.EntityFrameworkCore;
using Model;
using Model.Registrations;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
  public class DescriptionFilesRepository : GenericRepository<DescriptionFiles>, IDescriptionFilesRepository
  {
    public DescriptionFilesRepository(ContextBase dbContext) : base(dbContext)
    {

    }
    public async Task<PagedResult<DescriptionFiles>> GetAllPaged(Filters filter)
    {
      var data = await (from descriptionFiles in base._dbContext.Set<DescriptionFiles>()
                        where (filter.selectOption == FilterType.Name && (string.IsNullOrEmpty(filter.textOption) ||
                        descriptionFiles.NameProduct.Contains(filter.textOption)) ||
                        filter.codGroup > 0 || descriptionFiles.groupItems == filter.codGroup)

                        //let files = (
                        //  from c in _dbContext.Set<File>() where descriptionFiles.Id == c.IdDescriptionFiles select c
                        // ).ToList()

                        select new DescriptionFiles
                        {
                          Id = descriptionFiles.Id,
                          idCompany = descriptionFiles.idCompany,
                          NameProduct = descriptionFiles.NameProduct,
                          descriptionProduct = descriptionFiles.descriptionProduct,
                          valueProduct = descriptionFiles.valueProduct,
                          groupItems = descriptionFiles.groupItems,
                          //Files = files
                        }).GetPagedAsync<DescriptionFiles>(filter.pageNumber, filter.pageSize);
      return data;

    }

    public async Task<PagedResult<DescriptionFiles>> GetSearchPaged(string name, int codGroup)
    {
      var data = await (from descriptionFiles in base._dbContext.Set<DescriptionFiles>()
                        where (string.IsNullOrEmpty(name) || descriptionFiles.NameProduct.Contains(name) &&
                        descriptionFiles.groupItems == codGroup)

                        let files = (
                          from c in _dbContext.Set<File>() where descriptionFiles.Id == c.IdDescriptionFiles select c
                         ).ToList()

                        select new DescriptionFiles
                        {
                          Id = descriptionFiles.Id,
                          idCompany = descriptionFiles.idCompany,
                          NameProduct = descriptionFiles.NameProduct,
                          descriptionProduct = descriptionFiles.descriptionProduct,
                          valueProduct = descriptionFiles.valueProduct,
                          groupItems = descriptionFiles.groupItems,
                          Files = files
                        }).GetPagedAsync<DescriptionFiles>(1, 10);
      return data;
    }
    public async Task<DescriptionFiles> GetById(int id)
    {
      var data = await base._dbContext.Set<DescriptionFiles>().Include(x => x.Files)
        .Where(a => a.Id == id).AsNoTracking().FirstOrDefaultAsync();
      return data;
    }

  }
  public interface IDescriptionFilesRepository : IGenericRepository<DescriptionFiles>
  {
    Task<PagedResult<DescriptionFiles>> GetAllPaged(Filters filters);
    Task<PagedResult<DescriptionFiles>> GetSearchPaged(string name, int codGroup);
    Task<DescriptionFiles> GetById(int id);
  }

}
