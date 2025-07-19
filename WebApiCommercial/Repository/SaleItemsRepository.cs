using Model;
using Model.Moves;
using Model.Registrations;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
  public class SaleItemsRepository : GenericRepository<SaleItems>, ISaleItemsRepository
  {
    public SaleItemsRepository(ContextBase dbContext) : base(dbContext)
    {
    }

    public async Task<PagedResult<SaleItems>> GetPaged(Filters filters)
    {

      var paged = await (from saleItems in _dbContext.Set<SaleItems>()
                         join product in base._dbContext.Set<Product>()
                         on saleItems.IdProduct equals product.Id into leftProduct
                         from prod in leftProduct.DefaultIfEmpty()
                         join service in base._dbContext.Set<ServiceProvided>()
                         on saleItems.IdService equals service.Id into leftService
                         from serv in leftService.DefaultIfEmpty()
                         join sharedc in base._dbContext.Set<SharedCommission>()
                      on saleItems.Id equals sharedc.IdSaleItems into leftSharedc
                         from sharedc in leftSharedc.DefaultIfEmpty()
                         where saleItems.IdSale == filters.IdSale
                         select new SaleItems
                         {
                           Id = saleItems.Id,
                           Amount = saleItems.Amount,
                           Value = saleItems.Value,
                           InclusionDate = saleItems.InclusionDate,
                           NameItem = prod == null ? serv.Name : prod.Name,
                           TypeItem = saleItems.TypeItem,
                           IdProduct = saleItems.IdProduct,
                           IdService = saleItems.IdService,
                           EnableRecurrence = saleItems.EnableRecurrence,
                           RecurringAmount = saleItems.RecurringAmount,
                           SharedCommissions = saleItems.SharedCommissions,
                         }).GetPagedAsync<SaleItems>(filters.pageNumber, filters.pageSize);
      return paged;
    }
  }
  public interface ISaleItemsRepository : IGenericRepository<SaleItems>
  {
    Task<PagedResult<SaleItems>> GetPaged(Filters filters);
  }
}
