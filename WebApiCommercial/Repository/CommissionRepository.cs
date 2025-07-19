using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO;
using Model.Moves;
using Model.Registrations;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Repository
{
  public class CommissionRepository : GenericRepository<Commission>, ICommissionRepository
  {
    public CommissionRepository(ContextBase dbContext) : base(dbContext)
    {

    }

    public async Task<PagedResult<CommissionResponse>> GetAllPagedServicesCommission(Filters filter)
    {
      try
      {
        var dados = await (from commission in base._dbContext.Set<Commission>()
                           join service in base._dbContext.Set<ServiceProvided>()
                           on commission.IdService equals service.Id
                           where commission.IdSalesman == filter.IdSeller
                           select
                            new CommissionResponse
                            {
                              IdService = (int)commission.IdService,
                              DescriptionService = service.Name,
                              Id = commission.Id,
                              Percentage = commission.Percentage,
                              Status = commission.Status,
                              CommissionDay = commission.CommissionDay,
                              IdCostCenter = commission.IdCostCenter,
                              TypeDay = (int)commission.TypeDay
                            })
                          .AsNoTracking()
                          .GetPagedAsync<CommissionResponse>(filter.pageNumber, filter.pageSize);
        return dados;
      }
      catch (System.Exception ex)
      {

        throw;
      }
    }
    public async Task<PagedResult<CommissionResponse>> GetAllPagedProductsCommission(Filters filter)
    {
      try
      {
        var dados = await (from commission in base._dbContext.Set<Commission>()
                         join product in base ._dbContext.Set<Product>()
                         on commission.IdProduct equals product.Id
                           where commission.IdSalesman == filter.IdSeller
                           select
                            new CommissionResponse
                            {
                              IdProduct = (int)commission.IdProduct,
                              DescriptionProduct = product.Name,
                              Id = commission.Id,
                              Percentage = commission.Percentage,
                              Status = commission.Status,
                              CommissionDay = commission.CommissionDay,
                              IdCostCenter = commission.IdCostCenter,
                              TypeDay = (int)commission.TypeDay
                            })
                          .AsNoTracking()
                          .GetPagedAsync<CommissionResponse>(filter.pageNumber, filter.pageSize);
        return dados;
      }
      catch (System.Exception ex)
      {

        throw;
      }
       
    }

    private static CommissionResponse commissionResponseProd(Product product,int id)
    {
      var commissionResponse = new CommissionResponse();
      if (product.Commissions.Count > 0)
      {
        foreach (var item in product.Commissions)
        {
          if (item.IdSalesman == id)
          {
            commissionResponse = new CommissionResponse
            {
              IdProduct = product.Id,
              DescriptionProduct = product.Name,
              Id = item.Id,
              Percentage = item.Percentage,
              Status = item.Status,
              CommissionDay = item.CommissionDay,
              IdCostCenter = item.IdCostCenter,
              TypeDay = (int)item.TypeDay
            };
            break;
          }
          else
          {
            commissionResponse = new CommissionResponse
            {
              IdProduct = product.Id,
              DescriptionProduct = product.Name,
            };
          }
        }
      }
      else
      {
        commissionResponse= new CommissionResponse
        {
          IdProduct = product.Id,
          DescriptionProduct = product.Name,
        };
      }
     return commissionResponse;
    }
    private static CommissionResponse commissionResponseServ(ServiceProvided service, int id)
    {
      var commissionResponse = new CommissionResponse();
      if (service.Commissions.Count > 0)
      {
        foreach (var item in service.Commissions)
        {
          if (item.IdSalesman == id)
          {
            commissionResponse = new CommissionResponse
            {
              IdService= service.Id,
              DescriptionService = service.Name,
              Id = item.Id,
              Percentage = item.Percentage,
              Status = item.Status,
              CommissionDay = item.CommissionDay,
              IdCostCenter = item.IdCostCenter,
              TypeDay = (int)item.TypeDay
            };
            break;
          }
          else
          {
            commissionResponse = new CommissionResponse
            {
              IdService = service.Id,
              DescriptionService = service.Name,
            };
          }
        }
      }
      else
      {
        commissionResponse = new CommissionResponse
        {
          IdService = service.Id,
          DescriptionService = service.Name,
        };
      }
      return commissionResponse;
    }
    public async Task<Commission> CheckCommissionService(int idService, int idSeller)
    {
      var data = await (from commission in base._dbContext.Set<Commission>()
                         .Include(x => x.Salesman)
                        where commission.IdService == idService
                        && commission.IdSalesman == idSeller
                        select commission).AsNoTracking().FirstOrDefaultAsync();
      return data;
    }
    public async Task<Commission> CheckCommissionProduct(int idProduct, int idSeller)
    {
      var data = await (from commission in base._dbContext.Set<Commission>()
                         .Include(x => x.Salesman)
                        where commission.IdProduct == idProduct
                        && commission.IdSalesman == idSeller
                        select commission).AsNoTracking().FirstOrDefaultAsync();
      return data;
    }
    public async Task<bool> GetByIdSalesman(int IdSalesman, int idItem, TypeItem typeItem)
    {
      return await _dbContext.Set<Commission>()
        .Where(x => x.IdSalesman == IdSalesman 
         && (typeItem == TypeItem.Product && x.IdProduct == idItem
        || typeItem == TypeItem.Service && x.IdService == idItem))
        .AsNoTracking().AnyAsync();
    }
  }

  public interface ICommissionRepository : IGenericRepository<Commission>
  {
    Task<PagedResult<CommissionResponse>> GetAllPagedServicesCommission(Filters filter);
    Task<PagedResult<CommissionResponse>> GetAllPagedProductsCommission(Filters filter);
    Task<Commission> CheckCommissionService(int idService, int idSeller);
    Task<Commission> CheckCommissionProduct(int idProduct, int idSeller);
    Task<bool> GetByIdSalesman(int IdSalesman, int idItem, TypeItem typeItem);
  }
}
