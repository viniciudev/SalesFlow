using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO;
using Model.Registrations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly IStockRepository _stockRepository;
        public ProductRepository(ContextBase dbContext, IStockRepository stockRepository) : base(dbContext)
        {
            _stockRepository = stockRepository;
        }

        public async Task<PagedResult<Product>> GetAllPaged(Filters filter)
        {

            var paged = await base._dbContext.Set<Product>()
                      .Where(x => x.IdCompany == filter.IdCompany && string.IsNullOrEmpty(filter.TextOption) || x.Name.Contains(filter.TextOption))
                   .Select(p => new Product
                   {
                       Id = p.Id,
                       Name = p.Name,
                       Value = p.Value,
                       Description = p.Description,
                       Code = p.Code,
                       IdCompany = p.IdCompany,
                       Reference = p.Reference,
                      CostPrice=p.CostPrice
                       //Image = p.Image,
                       // ImageBytes = p.Image != null ? Convert.ToBase64String(p.Image) : null
                   })
                   .WithCaseInsensitive()
              .GetPagedAsync<Product>(filter.PageNumber, filter.PageSize);
            //busca saldo
            foreach (var item in paged.Results)
            {
                StockSummary stockSummary = await _stockRepository.GetBalanceByProduct(filter.IdCompany, item.Id);
                item.Quantity = stockSummary.SaldoAtual;
            }
            return paged;
        }
        public async Task<List<Product>> GetListByName(Filters filters)
        {
            var data = await base._dbContext.Set<Product>().Where(x =>
            x.IdCompany == filters.IdCompany &&
            (string.IsNullOrEmpty(filters.TextOption) 
            || x.Name.Contains(filters.TextOption)
            || x.Reference.Contains(filters.TextOption)
            || x.Code.Contains(filters.TextOption)))
              .AsNoTracking().WithCaseInsensitive().ToListAsync();

            //busca saldo
            foreach (var item in data)
            {
                StockSummary stockSummary = await _stockRepository.GetBalanceByProduct(filters.IdCompany, item.Id);
                item.Quantity = stockSummary.SaldoAtual;
            }
            return data;
        }
        public async Task<int> GetCountProductsByIdCompany(int tenantid)
        {
            var data = await base._dbContext.Set<Product>().Where(x =>
          x.IdCompany == tenantid)
            .AsNoTracking().CountAsync();
            return data;
        }
    }
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<PagedResult<Product>> GetAllPaged(Filters filter);
        Task<List<Product>> GetListByName(Filters filters);
        Task<int> GetCountProductsByIdCompany(int tenantid);
    }
}
