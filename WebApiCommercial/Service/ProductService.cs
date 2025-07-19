using Model;
using Model.Registrations;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
  public class ProductService : BaseService<Product>, IProductService
  {
    public ProductService(IGenericRepository<Product> repository) : base(repository)
    {
    }

    public async Task<PagedResult<Product>> GetAllPaged(Filters filter)
    {
      return await(repository as IProductRepository).GetAllPaged(filter);
    }
    public async Task<List<Product>> GetListByName(Filters filters)
    {
      return await (repository as IProductRepository).GetListByName(filters);
    }
  }
  public interface IProductService : IBaseService<Product>
  {
    Task<PagedResult<Product>> GetAllPaged(Filters filter);
    Task<List<Product>> GetListByName(Filters filters);
  }
}
