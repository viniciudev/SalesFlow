using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO;
using Model.Moves;
using Model.Registrations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    public class PurchaseRepository : GenericRepository<Purchase>, IPurchaseRepository
    {
        public PurchaseRepository(ContextBase dbContext) : base(dbContext)
        {
        }

        public async Task<PagedResult<Purchase>> GetAllPaged(Filters filters)
        {
            var paged = await (from compra in base._dbContext.Set<Purchase>()
                               .Include(x => x.Fornecedor)
                               .Include(x => x.PurchaseItems)
                               where compra.IdCompany == filters.IdCompany
                               && (string.IsNullOrEmpty(filters.TextOption)
                                   || compra.Fornecedor.nome.Contains(filters.TextOption)
                                   || compra.ChaveNfe.Contains(filters.TextOption))
                               && (filters.SaleDate == default || compra.DataCompra >= filters.SaleDate)
                               && (filters.SaleDateFinal == default || compra.DataCompra <= filters.SaleDateFinal)
                               orderby compra.DataCompra descending
                               select new Purchase
                               {
                                   Id = compra.Id,
                                   DataEntrada = compra.DataEntrada,
                                   DataCompra = compra.DataCompra,
                                   ChaveNfe = compra.ChaveNfe,
                                   FornecedorId = compra.FornecedorId,
                                   NomeFornecedor = compra.Fornecedor.nome,
                                   ValorTotal = compra.ValorTotal,
                                   DataCadastro = compra.DataCadastro,
                                   IdCompany = compra.IdCompany
                               })
                               .GetPagedAsync<Purchase>(filters.PageNumber, filters.PageSize);
            return paged;
        }

        public async Task<Purchase> GetByIdWithItems(int id)
        {
            return await _dbContext.Set<Purchase>()
                .Include(x => x.Fornecedor)
                .Include(x => x.PurchaseItems)
                    .ThenInclude(x => x.Produto)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }

    public interface IPurchaseRepository : IGenericRepository<Purchase>
    {
        Task<PagedResult<Purchase>> GetAllPaged(Filters filters);
        Task<Purchase> GetByIdWithItems(int id);
    }
}
