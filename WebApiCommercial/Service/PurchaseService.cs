using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO;
using Model.Moves;
using Model.Registrations;
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
		private readonly IPaymentMethodRepository _paymentMethodRepository;
		public PurchaseService(IGenericRepository<Purchase> repository, ContextBase dbContext, IStockService stockService,
			IFinancialService financialService, IBoxRepository boxRepository,
			ICostCenterRepository costCenterRepository,
			IFinancialPaymentMethodRepository financialPaymentMethodRepository,
			IPaymentMethodRepository paymentMethodRepository) : base(repository)
		{
			_dbContext = dbContext;
			_stockService = stockService;
			_financialService = financialService;
			_boxRepository = boxRepository;
			_costCenterRepository = costCenterRepository;
			_financialPaymentMethodRepository = financialPaymentMethodRepository;
			_paymentMethodRepository = paymentMethodRepository;
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
			if (formPayment == null || !formPayment.Any())
				return;

			// BUSCAR DADOS
			var caixaAberto = await _boxRepository.GetByStatus(CaixaStatus.ABERTO, IdCompany);
		

			var listCostCenter = await _costCenterRepository.GetByIdCompany(IdCompany);
			var costCenterId = listCostCenter.FirstOrDefault()?.Id;

			// VALIDAR
			var paymentMethods = new Dictionary<int, PaymentMethod>();
			decimal totalValue = 0;
			bool hasPendingPayment = false;

			foreach (var m in formPayment)
			{
				totalValue += m.Value;

				var paymentMethod = await _paymentMethodRepository.GetByIdAsync(m.PaymentMethodId);
				if (paymentMethod == null)
					throw new Exception($"Método de pagamento ID {m.PaymentMethodId} não encontrado");

				paymentMethods[m.PaymentMethodId] = paymentMethod;

				if (!paymentMethod.IsImmediateSettlement)
					hasPendingPayment = true;

				if (m.Installments.HasValue && m.Installments.Value > 1)
				{
					if (!paymentMethod.AllowInstallments)
						throw new Exception($"O método {paymentMethod.Name} não permite parcelamento");
					hasPendingPayment = true;
				}
				else
				{
					m.Installments = 1;
				}
			}

			decimal finalValue = troco.HasValue ? totalValue - troco.Value : totalValue;

			// CRIAR FINANCEIROS
			var financialsToCreate = new List<Financial>();

			foreach (var m in formPayment)
			{
				var paymentMethod = paymentMethods[m.PaymentMethodId];
				int installments = m.Installments ?? 1;
				decimal installmentValue = Math.Round(m.Value / installments, 2);
				decimal remainingValue = m.Value;

				DateTime firstDueDate = GetFirstDueDate();

				for (int i = 0; i < installments; i++)
				{
					decimal currentValue = (i == installments - 1)
							? Math.Round(remainingValue, 2)
							: installmentValue;
					remainingValue -= currentValue;

					DateTime dueDate = AdjustToBusinessDay(firstDueDate.AddMonths(i));

					// DEFINE STATUS
					bool isPaid = paymentMethod.IsImmediateSettlement && installments == 1;
					FinancialStatus status = isPaid ? FinancialStatus.paid : FinancialStatus.pending;

					var financial = new Financial
					{
						Id = 0,
						FinancialStatus = status,
						FinancialType = FinancialType.expense, // COMPRA
						Origin = OriginFinancial.financial,
						IdPurchase = IdPurchase, // FK PARA COMPRA
						CreationDate = DateTime.Now,
						DueDate = dueDate,
						SettlementDate = isPaid ? DateTime.Now.ToString() : null,
						IdCompany = IdCompany,
						BoxId = caixaAberto?.Id,
						Description = installments > 1
									? $"Parcela {i + 1}/{installments} - {paymentMethod.Name} - Compra #{IdPurchase}"
									: $"{paymentMethod.Name} - Compra #{IdPurchase}",
						IdCostCenter = costCenterId,
						IdClient = IdClient,
						Value = currentValue,
						Troco = null,
						BankAccountId = BankAccountId,
						FinancialPaymentMethods = new List<FinancialPaymentMethod>
								{
										new FinancialPaymentMethod
										{
												PaymentMethodId = m.PaymentMethodId,
												FinancialId = 0,
												Amount = currentValue,
												Installments = 1
										}
								}
					};

					await _financialService.Create(financial);
				}
			}


		}

		private DateTime GetFirstDueDate()
		{
			return AdjustToBusinessDay(DateTime.Now.AddDays(30));
		}

		private DateTime AdjustToBusinessDay(DateTime date)
		{
			if (date.DayOfWeek == DayOfWeek.Saturday)
				return date.AddDays(2);
			if (date.DayOfWeek == DayOfWeek.Sunday)
				return date.AddDays(1);
			return date;
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
