using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO;
using Model.Moves;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service
{
	public class PurchaseService : BaseService<Purchase>, IPurchaseService
	{
		private readonly ContextBase _dbContext;
		private readonly IStockService _stockService;
		private readonly IFinancialService _financialService;
		private readonly IBoxRepository _boxRepository;
		private readonly ICostCenterRepository _costCenterRepository;
		private readonly IFinancialPaymentMethodRepository _financialPaymentMethodRepository;
		public PurchaseService(IGenericRepository<Purchase> repository, ContextBase dbContext, IStockService stockService, IFinancialService financialService, IBoxRepository boxRepository, ICostCenterRepository costCenterRepository, IFinancialPaymentMethodRepository financialPaymentMethodRepository) : base(repository)
		{
			_dbContext = dbContext;
			_stockService = stockService;
			_financialService = financialService;
			_boxRepository = boxRepository;
			_costCenterRepository = costCenterRepository;
			_financialPaymentMethodRepository = financialPaymentMethodRepository;
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
						await _dbContext.SaveChangesAsync();

						await _stockService.Create(new Stock
						{
							IdCompany = purchaseDto.IdCompany,
							Quantity = item.Quantidade,
							Date = purchaseDto.DataEntrada,
							IdProduct = (int)item.ProdutoId,
							Reason = $"Compra: dia {purchaseDto.DataEntrada}",
							Type = StockType.entry,
							ReferenceId = purchaseItem.Id,
						});

					}
			
					 await GenerateFinancial(purchaseDto.FormPayment, compra.Id, purchaseDto.IdCompany, null, purchaseDto.BankAccountId, purchaseDto.Troco);


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

						Stock stock = await _stockService.GetByReferenceIdAsync(item.Id, compra.IdCompany, StockType.entry);
						//deletar para inserir com os novos itens
						if (stock != null)
						{
							await _stockService.DeleteAsync(stock.Id);
						}
					}
					//deletar financeiros
					List<Financial> fin = await _financialService.GetByIdPurchaseAsync(purchaseDto.Id);
					foreach (var item in fin)
					{
						foreach (var forms in item.FinancialPaymentMethods)
						{
							await _financialPaymentMethodRepository.DeleteAsync(forms.Id);
						}
						await _financialService.DeleteAsync(item.Id);
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
						await _dbContext.SaveChangesAsync();

						await _stockService.Create(new Stock
						{
							IdCompany = purchaseDto.IdCompany,
							Quantity = item.Quantidade,
							Date = purchaseDto.DataEntrada,
							IdProduct = (int)item.ProdutoId,
							Reason = $"Compra: dia {purchaseDto.DataEntrada}",
							Type = StockType.entry,
							ReferenceId = purchaseItem.Id,
						});
					}
					await GenerateFinancial(purchaseDto.FormPayment, compra.Id, purchaseDto.IdCompany, null, purchaseDto.BankAccountId, purchaseDto.Troco);	


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
		private async Task GenerateFinancial(
			ICollection<FormPaymentPurchase> formPayment,
			int IdPurchase, int IdCompany,
			int? IdClient = null, int? BankAccountId = null, decimal? troco = null)
		{
			if (formPayment != null && formPayment.Count() > 0)
			{
				var caixaAberto = await _boxRepository.GetByStatus(CaixaStatus.ABERTO, IdCompany);
				var listCostCenter = await _costCenterRepository.GetByIdCompany(IdCompany);
				decimal value = troco != null ? (decimal)(formPayment.Sum(x => x.Value) - troco) : formPayment.Sum(x => x.Value);
				Financial item = new Financial();
				item.Id = 0;
				item.FinancialStatus = FinancialStatus.paid;
				item.FinancialType = FinancialType.expense;
				item.Origin = OriginFinancial.financial;
				item.IdPurchase = IdPurchase;
				item.CreationDate = DateTime.Now;
				item.DueDate = DateTime.Now;
				item.SettlementDate = DateTime.Now.ToString();
				item.IdCompany = IdCompany;
				item.BoxId = caixaAberto != null ? caixaAberto.Id : null;
				item.Description = $"Compra no dia:{DateTime.Now}";
				item.IdCostCenter = listCostCenter.FirstOrDefault()?.Id;
				item.IdClient = IdClient;
				item.Value = value;
				item.Troco = troco;
				item.BankAccountId = BankAccountId;

				List<FinancialPaymentMethod> financialPaymentMethod = new();
				foreach (var m in formPayment)
				{
					financialPaymentMethod.Add(new FinancialPaymentMethod
					{
						PaymentMethodId = m.PaymentMethodId,
						FinancialId = item.Id,
						Amount = m.Value,
						//      Installments = item.Installments
					});
				}
				item.FinancialPaymentMethods = financialPaymentMethod;
				await _financialService.Create(item);
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
