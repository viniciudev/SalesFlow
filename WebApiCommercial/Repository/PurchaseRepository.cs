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
                .Where(x => x.Id == id)
                .Select(p => new Purchase
                {
                    Id = p.Id,
                    IdCompany = p.IdCompany,
                    DataEntrada = p.DataEntrada,
                    DataCompra = p.DataCompra,
                    ChaveNfe = p.ChaveNfe,
                    FornecedorId = p.FornecedorId,
                    ValorTotal = p.ValorTotal,
                    DataCadastro = p.DataCadastro,
                    NomeFornecedor = p.Fornecedor.nome,
                    Fornecedor = new Provider
                    {
                        Id = p.Fornecedor.Id,
                        nome = p.Fornecedor.nome,
                        razaoSocial = p.Fornecedor.razaoSocial,
                        nomeFantasia = p.Fornecedor.nomeFantasia,
                        cnpj = p.Fornecedor.cnpj,
                        inscricaoEstadual = p.Fornecedor.inscricaoEstadual,
                        telefone = p.Fornecedor.telefone,
                        email = p.Fornecedor.email,
                        logradouro = p.Fornecedor.logradouro,
                        numero = p.Fornecedor.numero,
                        bairro = p.Fornecedor.bairro,
                        cidade = p.Fornecedor.cidade,
                        uf = p.Fornecedor.uf,
                        cep = p.Fornecedor.cep,
                        complemento = p.Fornecedor.complemento,
                        idcnae = p.Fornecedor.idcnae,
                        IdCompany = p.Fornecedor.IdCompany,
                    },
                    PurchaseItems = p.PurchaseItems.Select(pi => new PurchaseItem
                    {
                        Id = pi.Id,
                        CompraId = pi.CompraId,
                        ProdutoId = pi.ProdutoId,
                        CodigoProduto = pi.CodigoProduto,
                        DescricaoProduto = pi.DescricaoProduto,
                        Quantidade = pi.Quantidade,
                        ValorUnitario = pi.ValorUnitario,
                        Desconto = pi.Desconto,
                        ValorTotal = pi.ValorTotal,
                        Produto = pi.Produto != null ? new Product
                        {
                            Id = pi.Produto.Id,
                            IdCompany = pi.Produto.IdCompany,
                            Name = pi.Produto.Name,
                            Value = pi.Produto.Value,
                            Quantity = pi.Produto.Quantity,
                            Description = pi.Produto.Description,
                            Observation = pi.Produto.Observation,
                            Code = pi.Produto.Code,
                            Reference = pi.Produto.Reference,
                            CostPrice = pi.Produto.CostPrice,
                        } : null,
                    }).ToList(),
                    Financials = p.Financials.Select(f => new Financial
                    {
                        Id = f.Id,
                        IdCompany = f.IdCompany,
                        CreationDate = f.CreationDate,
                        Description = f.Description,
                        FinancialType = f.FinancialType,
                        IdCostCenter = f.IdCostCenter,
                        FinancialStatus = f.FinancialStatus,
                        DueDate = f.DueDate,
                        Value = f.Value,
                        IdPurchase = f.IdPurchase,
                        Percentage = f.Percentage,
                        commission = f.commission,
                        Origin = f.Origin,
                        BoxId = f.BoxId,
                        BankAccountId = f.BankAccountId,
                        BankAccountName = f.BankAccount != null
                            ? f.BankAccount.BankName + " " + f.BankAccount.BankCode + "-" + f.BankAccount.AccountNumber + "/" + f.BankAccount.AccountDigit
                            : null,
                        SettlementDate = f.SettlementDate,
                        InterestValue = f.InterestValue,
                        FineValue = f.FineValue,
                        SettledValue = f.SettledValue,
                        Troco = f.Troco,
                        FinancialPaymentMethods = f.FinancialPaymentMethods.Select(fpm => new FinancialPaymentMethod
                        {
                            Id = fpm.Id,
                            FinancialId = fpm.FinancialId,
                            PaymentMethodId = fpm.PaymentMethodId,
                            Amount = fpm.Amount,
                            PaymentMethodName = fpm.PaymentMethod != null ? fpm.PaymentMethod.Name : null,
                        }).ToList(),
                    }).ToList(),
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }

    public interface IPurchaseRepository : IGenericRepository<Purchase>
    {
        Task<PagedResult<Purchase>> GetAllPaged(Filters filters);
        Task<Purchase> GetByIdWithItems(int id);
    }
}
