using Model;
using Model.DTO;
using Model.Moves;
using Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service
{
    public class PurchaseService : BaseService<Purchase>, IPurchaseService
    {
        private readonly ContextBase _dbContext;

        public PurchaseService(IGenericRepository<Purchase> repository, ContextBase dbContext) : base(repository)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<Purchase>> GetAllPaged(Filters filters)
        {
            return await (repository as IPurchaseRepository).GetAllPaged(filters);
        }

        public async Task<Purchase> GetByIdWithItems(int id)
        {
            return await (repository as IPurchaseRepository).GetByIdWithItems(id);
        }

        public async Task<int> SaveWithItems(PurchaseDto purchaseDto)
        {
            using (var transaction = await repository.CreateTransactionAsync())
            {
                try
                {
                    if (string.IsNullOrEmpty(purchaseDto.ChaveNfe) || purchaseDto.ChaveNfe.Length != 44)
                        throw new ArgumentException("Chave da NF-e deve possuir exatamente 44 caracteres.");

                    if (purchaseDto.FornecedorId <= 0)
                        throw new ArgumentException("Fornecedor eh obrigatorio.");

                    if (purchaseDto.PurchaseItems == null || !purchaseDto.PurchaseItems.Any())
                        throw new ArgumentException("A compra deve ter pelo menos um item.");

                    foreach (var item in purchaseDto.PurchaseItems)
                    {
                        if (item.Quantidade <= 0)
                            throw new ArgumentException("Quantidade do item deve ser maior que zero.");

                        if (item.ValorUnitario <= 0)
                            throw new ArgumentException("Valor unitario do item deve ser maior que zero.");

                        item.ValorTotal = (item.Quantidade * item.ValorUnitario) - item.Desconto;
                    }

                    purchaseDto.ValorTotal = purchaseDto.PurchaseItems.Sum(x => x.ValorTotal);

                    Purchase compra = new Purchase
                    {
                        IdCompany = purchaseDto.IdCompany,
                        DataEntrada = purchaseDto.DataEntrada,
                        DataCompra = purchaseDto.DataCompra,
                        ChaveNfe = purchaseDto.ChaveNfe,
                        FornecedorId = purchaseDto.FornecedorId,
                        ValorTotal = purchaseDto.ValorTotal,
                        DataCadastro = DateTime.Now
                    };

                    await base.Save(compra);

                    foreach (var item in purchaseDto.PurchaseItems)
                    {
                        PurchaseItem purchaseItem = new PurchaseItem
                        {
                            CompraId = compra.Id,
                            ProdutoId = item.ProdutoId,
                            CodigoProduto = item.CodigoProduto,
                            DescricaoProduto = item.DescricaoProduto,
                            Quantidade = item.Quantidade,
                            ValorUnitario = item.ValorUnitario,
                            Desconto = item.Desconto,
                            ValorTotal = item.ValorTotal
                        };

                        await _dbContext.Set<PurchaseItem>().AddAsync(purchaseItem);
                    }

                    await _dbContext.SaveChangesAsync();
                    transaction.Commit();
                    return compra.Id;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<int> UpdateWithItems(PurchaseDto purchaseDto)
        {
            using (var transaction = await repository.CreateTransactionAsync())
            {
                try
                {
                    if (string.IsNullOrEmpty(purchaseDto.ChaveNfe) || purchaseDto.ChaveNfe.Length != 44)
                        throw new ArgumentException("Chave da NF-e deve possuir exatamente 44 caracteres.");

                    if (purchaseDto.FornecedorId <= 0)
                        throw new ArgumentException("Fornecedor eh obrigatorio.");

                    if (purchaseDto.PurchaseItems == null || !purchaseDto.PurchaseItems.Any())
                        throw new ArgumentException("A compra deve ter pelo menos um item.");

                    foreach (var item in purchaseDto.PurchaseItems)
                    {
                        if (item.Quantidade <= 0)
                            throw new ArgumentException("Quantidade do item deve ser maior que zero.");

                        if (item.ValorUnitario <= 0)
                            throw new ArgumentException("Valor unitario do item deve ser maior que zero.");

                        item.ValorTotal = (item.Quantidade * item.ValorUnitario) - item.Desconto;
                    }

                    purchaseDto.ValorTotal = purchaseDto.PurchaseItems.Sum(x => x.ValorTotal);

                    Purchase compra = new Purchase
                    {
                        Id = purchaseDto.Id,
                        IdCompany = purchaseDto.IdCompany,
                        DataEntrada = purchaseDto.DataEntrada,
                        DataCompra = purchaseDto.DataCompra,
                        ChaveNfe = purchaseDto.ChaveNfe,
                        FornecedorId = purchaseDto.FornecedorId,
                        ValorTotal = purchaseDto.ValorTotal,
                        DataCadastro = DateTime.Now
                    };

                    await base.Alter(compra);

                    var existingItems = await _dbContext.Set<PurchaseItem>()
                        .Where(x => x.CompraId == compra.Id).ToListAsync();

                    foreach (var item in existingItems)
                    {
                        _dbContext.Set<PurchaseItem>().Remove(item);
                    }

                    foreach (var item in purchaseDto.PurchaseItems)
                    {
                        PurchaseItem purchaseItem = new PurchaseItem
                        {
                            CompraId = compra.Id,
                            ProdutoId = item.ProdutoId,
                            CodigoProduto = item.CodigoProduto,
                            DescricaoProduto = item.DescricaoProduto,
                            Quantidade = item.Quantidade,
                            ValorUnitario = item.ValorUnitario,
                            Desconto = item.Desconto,
                            ValorTotal = item.ValorTotal
                        };

                        await _dbContext.Set<PurchaseItem>().AddAsync(purchaseItem);
                    }

                    await _dbContext.SaveChangesAsync();
                    transaction.Commit();
                    return compra.Id;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }

    public interface IPurchaseService : IBaseService<Purchase>
    {
        Task<PagedResult<Purchase>> GetAllPaged(Filters filters);
        Task<Purchase> GetByIdWithItems(int id);
        Task<int> SaveWithItems(PurchaseDto purchaseDto);
        Task<int> UpdateWithItems(PurchaseDto purchaseDto);
    }
}
