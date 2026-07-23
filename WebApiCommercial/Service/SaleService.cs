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
	public class SaleService : BaseService<Sale>, ISaleService
	{
		private readonly ISaleItemsService saleItemsService;
		private readonly ICommissionService commissionService;
		private readonly IStockService _stockService;
		private readonly ICostCenterRepository _costCenterRepository;
		private readonly IFinancialService _financialService;
		private readonly IFinancialPaymentMethodRepository _financialPaymentMethodRepository;
		private readonly IBoxRepository _boxRepository;
		private readonly INFeRepository _nfeRepository;
		private readonly IPaymentMethodRepository _paymentMethodRepository;
		private readonly ISalePaymentRepository _salePaymentRepository;
		public SaleService(IGenericRepository<Sale> repository,
			ISaleItemsService saleItemsService,
			ICommissionService commissionService,
			IStockService stockService,
			ICostCenterRepository costCenterRepository,
			IFinancialService financialService,
			IFinancialPaymentMethodRepository financialPaymentMethodRepository,
			IBoxRepository boxRepository,
			INFeRepository nfeRepository,
			IPaymentMethodRepository paymentMethodRepository,
			ISalePaymentRepository salePaymentRepository) : base(repository)
		{
			this.saleItemsService = saleItemsService;
			this.commissionService = commissionService;
			_stockService = stockService;
			_costCenterRepository = costCenterRepository;
			_financialPaymentMethodRepository = financialPaymentMethodRepository;
			_financialService = financialService;
			_boxRepository = boxRepository;
			_nfeRepository = nfeRepository;
			_paymentMethodRepository = paymentMethodRepository;
			_salePaymentRepository = salePaymentRepository;
		}

		public async Task<PagedResult<Sale>> GetAllPaged(Filters filters)
		{
			return await (repository as ISaleRepository).GetAllPaged(filters);
		}
		public async Task<int> SaveWithItems(SaleDto sale)
		{
			using (var transaction = await repository.CreateTransactionAsync())
			{
				try
				{
					Sale s = new Sale
					{
						IdClient = sale.IdClient,
						IdCompany = sale.IdCompany,
						IdSeller = sale.IdSeller == 0 ? null : sale.IdSeller,
						ReleaseDate = sale.ReleaseDate,
						SaleDate = sale.SaleDate,
						Total = sale.Total,
						Status = SaleStatus.completed,
						//SalesOrder = sale.SalesOrder
					};
					await base.Save(s);

					SaleItems data = new SaleItems();
					SharedCommission sharedCommission = new SharedCommission();
					foreach (var item in sale.SaleItems)
					{
						item.IdSale = s.Id;
						data = new SaleItems
						{
							IdSale = item.IdSale,
							IdProduct = item.IdProduct == 0 ? null : item.IdProduct,
							IdService = item.IdService == 0 ? null : item.IdService,
							Value = item.Value,
							Amount = item.Amount,
							InclusionDate = item.InclusionDate,
							TypeItem = item.TypeItem,
							EnableRecurrence = item.EnableRecurrence,
							RecurringAmount = item.RecurringAmount,
						};
						await saleItemsService.Save(data);

						await _stockService.Create(new Stock
						{
							IdCompany = sale.IdCompany,
							Quantity = item.Amount,
							Date = sale.SaleDate,
							IdProduct = (int)item.IdProduct,
							Reason = $"Venda: dia {sale.SaleDate}",
							Type = StockType.exit,
							ReferenceId = data.Id
						});

						if (item.SharedCommissions != null && item.SharedCommissions.Count > 0)
							sharedCommission = new SharedCommission
							{
								Id = item.SharedCommissions.First().Id,
								CommissionDay = item.SharedCommissions.First().CommissionDay,
								IdCostCenter = item.SharedCommissions.First().IdCostCenter,
								IdSaleItems = item.Id,
								Percentage = item.SharedCommissions.First().Percentage,
								EnableSharedCommission = item.SharedCommissions.First().EnableSharedCommission,
								IdSalesman = item.SharedCommissions.First().IdSalesman,
								NameSeller = item.SharedCommissions.First().NameSeller,
								RecurringAmount = item.SharedCommissions.First().RecurringAmount,
								TypeDay = item.SharedCommissions.First().TypeDay,
							};
					}
					if (sale.IdSeller != null)
						await commissionService.GenerateCommission(data, sharedCommission, (int)sale.IdSeller, sale.IdCompany);
					//if (!sale.SalesOrder)
					//{
					await GenerateFinancial(sale.FormPaymentSales, s.Id, sale.IdCompany,
						sale.IdClient, sale.BankAccountId, sale.Troco);
					//}

					transaction.Commit();
					return s.Id;
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					throw;
				}
			}

		}
		private async Task GenerateFinancial(ICollection<FormPaymentSale> formPaymentSales, int IdSale, int IdCompany,
				int? IdClient = null, int? BankAccountId = null, decimal? troco = null)
		{
			if (formPaymentSales == null || !formPaymentSales.Any())
				return;

			var caixaAberto = await _boxRepository.GetByStatus(CaixaStatus.ABERTO, IdCompany);

			var listCostCenter = await _costCenterRepository.GetByIdCompany(IdCompany);
			var costCenterId = listCostCenter.FirstOrDefault()?.Id;

			var paymentMethods = new Dictionary<int, PaymentMethod>();
			decimal totalValue = 0;
			bool hasPendingPayment = false;

			foreach (var m in formPaymentSales)
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
					hasPendingPayment = true;
				}
				else
				{
					m.Installments = 1;
				}
			}

			decimal finalValue = troco.HasValue ? totalValue - troco.Value : totalValue;

			foreach (var m in formPaymentSales)
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

					DateTime dueDate = paymentMethod.IsImmediateSettlement && installments==1?DateTime.Now:
						AdjustToBusinessDay(firstDueDate.AddMonths(i));

					bool isPaid = paymentMethod.IsImmediateSettlement && installments == 1;
					FinancialStatus status = isPaid ? FinancialStatus.paid : FinancialStatus.pending;

					var financial = new Financial
					{
						Id = 0,
						FinancialStatus = status,
						FinancialType = FinancialType.recipe,
						Origin = OriginFinancial.financial,
						IdSale = IdSale,
						CreationDate = DateTime.Now,
						DueDate = dueDate,
						SettlementDate = isPaid ? DateTime.Now.ToString() : null,
						IdCompany = IdCompany,
						BoxId = caixaAberto?.Id,
						Description = installments > 1
									? $"Parcela {i + 1}/{installments} - {paymentMethod.Name} - Venda #{IdSale}"
									: $"{paymentMethod.Name} - Venda #{IdSale}",
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
		public async Task<int> PutWithItems(SaleDto sale)
		{
			using (var transaction = await repository.CreateTransactionAsync())
			{
				try
				{
					Sale existingSale = await base.GetByIdAsync(sale.Id);
					//bool isApprovedSalesOrder = existingSale != null
					//	&& existingSale.SalesOrder
					//	&& existingSale.Status == SaleStatus.Approved;

					Sale s = new Sale
					{
						Id = sale.Id,
						IdClient = sale.IdClient,
						IdCompany = sale.IdCompany,
						IdSeller = sale.IdSeller == 0 ? null : sale.IdSeller,
						ReleaseDate = sale.ReleaseDate,
						SaleDate = sale.SaleDate,
						Total = sale.Total,
						//SalesOrder = sale.SalesOrder,
						Status = SaleStatus.completed
						//isApprovedSalesOrder ? SaleStatus.Approved
						//	: (sale.SalesOrder ? SaleStatus.pending : SaleStatus.completed),
					};
					await base.Alter(s);

					SaleItems data = new SaleItems();
					SharedCommission sharedCommission = new SharedCommission();
					List<SaleItems> prods = await saleItemsService.GetByIdSaleAsync(s.Id);
					foreach (var item in prods)
					{
						if (item.Id > 0)
							await saleItemsService.DeleteAsync(item.Id);
						Stock stock = await _stockService.GetByReferenceIdAsync(item.Id, sale.IdCompany, StockType.exit);
						if (stock != null)
						{
							await _stockService.DeleteAsync(stock.Id);
						}
					}
					//if (!sale.SalesOrder)
					//{
					var fin = await _financialService.GetByIdSaleAsync(sale.Id);
					foreach (var item in fin)
					{
						foreach (var forms in item.FinancialPaymentMethods)
						{
							await _financialPaymentMethodRepository.DeleteAsync(forms.Id);
						}
						await _financialService.DeleteAsync(item.Id);
					}

					await GenerateFinancial(sale.FormPaymentSales, s.Id, sale.IdCompany,
						sale.IdClient, sale.BankAccountId, sale.Troco);
					//}

					if (sale.SalesOrder && sale.SalePayments != null && sale.SalePayments.Any())
					{
						await _salePaymentRepository.DeleteBySaleIdAsync(s.Id);
						foreach (var sp in sale.SalePayments)
						{
							sp.IdSale = s.Id;
							sp.Id = 0;
							sp.Status = SalePaymentStatus.Planned;
							await _salePaymentRepository.CreateAsync(sp);
						}
					}

					foreach (var item in sale.SaleItems)
					{
						item.IdSale = s.Id;
						data = new SaleItems
						{
							IdSale = item.IdSale,
							IdProduct = item.IdProduct == 0 ? null : item.IdProduct,
							IdService = item.IdService == 0 ? null : item.IdService,
							Value = item.Value,
							Amount = item.Amount,
							InclusionDate = item.InclusionDate,
							TypeItem = item.TypeItem,
							EnableRecurrence = item.EnableRecurrence,
							RecurringAmount = item.RecurringAmount,
						};
						await saleItemsService.Save(data);

						//if (!isApprovedSalesOrder)
						//{
						await _stockService.Create(new Stock
						{
							IdCompany = sale.IdCompany,
							Quantity = item.Amount,
							Date = sale.SaleDate,
							IdProduct = (int)item.IdProduct,
							Reason = $"Venda: dia {sale.SaleDate}",
							Type = StockType.exit,
							ReferenceId = data.Id,
						});
						//}

						if (item.SharedCommissions != null && item.SharedCommissions.Count > 0)
							sharedCommission = new SharedCommission
							{
								Id = item.SharedCommissions.First().Id,
								CommissionDay = item.SharedCommissions.First().CommissionDay,
								IdCostCenter = item.SharedCommissions.First().IdCostCenter,
								IdSaleItems = item.Id,
								Percentage = item.SharedCommissions.First().Percentage,
								EnableSharedCommission = item.SharedCommissions.First().EnableSharedCommission,
								IdSalesman = item.SharedCommissions.First().IdSalesman,
								NameSeller = item.SharedCommissions.First().NameSeller,
								RecurringAmount = item.SharedCommissions.First().RecurringAmount,
								TypeDay = item.SharedCommissions.First().TypeDay,
							};
					}
					if (sale.IdSeller != null)
						await commissionService.GenerateCommission(data, sharedCommission, (int)sale.IdSeller, sale.IdCompany);
					transaction.Commit();
					return s.Id;
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					throw;
				}
			}

		}
		public async Task<Sale> GetByIdSale(int id)
		{
			return await (repository as ISaleRepository).GetByIdSale(id);
		}
		public async Task<SaleInfoResponse> GetByMonthAllSales(Filters filters)
		{
			return await (repository as ISaleRepository).GetByMonthAllSales(filters);
		}
		public async Task<SalesCommissionsInfo> GetByWeekAllSales(Filters filters)
		{
			return await (repository as ISaleRepository).GetByWeekAllSales(filters);
		}
		public async Task<List<SalesmanInfo>> GetSalesmanByWeek(int idCompany)
		{
			return await (repository as ISaleRepository).GetSalesmanByWeek(idCompany);
		}
		public async Task<ResponseGeneric> Cancel(int saleId)
		{
			using (var transaction = await repository.CreateTransactionAsync())
			{
				try
				{
					var sale = await base.GetByIdAsync(saleId);
					if (sale == null)
						return new ResponseGeneric { Success = false, Data = "Venda não encontrada" };
					if (sale.Status == SaleStatus.canceled)
					{
						return new ResponseGeneric { Success = false, Data = "Venda já foi cancelada!" };
					}
					List<NFeEmission> nFeEmissionList = await _nfeRepository.GetBySaleIdAsync(saleId);

					if (nFeEmissionList.Count > 0)
					{
						NFeEmission nFeEmission = nFeEmissionList.FirstOrDefault(x => x.StatusNfe == StatusNfe.emitida);
						if (nFeEmission != null)
						{
							return new ResponseGeneric { Success = false, Data = "Não é possível cancelar venda com nota fiscal" };
						}
					}
					var saleItems = await saleItemsService.GetByIdSaleAsync(saleId);

					if (sale.SalesOrder && sale.Status == SaleStatus.Approved)
					{
						var salePayments = await _salePaymentRepository.GetBySaleIdAsync(saleId);
						foreach (var sp in salePayments)
						{
							sp.Status = SalePaymentStatus.Cancelled;
							await _salePaymentRepository.UpdateAsync(sp.Id, sp);
						}

						sale.Status = SaleStatus.canceled;
						await base.Alter(sale);
						transaction.Commit();
						return new ResponseGeneric { Success = true };
					}

					foreach (var item in saleItems)
					{
						var stock = await _stockService.GetByReferenceIdAsync(item.Id, sale.IdCompany, StockType.exit);
						if (stock != null)
						{
							await _stockService.Create(new Stock
							{
								IdCompany = sale.IdCompany,
								Quantity = item.Amount,
								Date = DateTime.Now,
								IdProduct = (int)item.IdProduct,
								Reason = $"Cancelamento de venda: {sale.Id}",
								Type = StockType.entry,
								ReferenceId = item.Id
							});
						}
					}

					var financials = await _financialService.GetByIdSaleAsync(saleId);
					foreach (var financial in financials)
					{
						if (financial.FinancialPaymentMethods != null)
						{
							foreach (var paymentMethod in financial.FinancialPaymentMethods)
							{
								await _financialPaymentMethodRepository.DeleteAsync(paymentMethod.Id);
							}
						}
						await _financialService.DeleteAsync(financial.Id);
					}

					sale.Status = SaleStatus.canceled;
					await base.Alter(sale);

					transaction.Commit();
					return new ResponseGeneric { Success = true };
				}
				catch
				{
					transaction.Rollback();
					throw;
				}
			}
		}

		public async Task<int> SaveSalesOrder(SaleDto sale)
		{
			using (var transaction = await repository.CreateTransactionAsync())
			{
				try
				{
					Sale s = new Sale
					{
						IdClient = sale.IdClient,
						IdCompany = sale.IdCompany,
						IdSeller = sale.IdSeller == 0 ? null : sale.IdSeller,
						ReleaseDate = sale.ReleaseDate,
						SaleDate = sale.SaleDate,
						Total = sale.Total,
						Status = SaleStatus.Approved,
						SalesOrder = true
					};
					await base.Save(s);

					foreach (var item in sale.SaleItems)
					{
						item.IdSale = s.Id;
						var saleItem = new SaleItems
						{
							IdSale = item.IdSale,
							IdProduct = item.IdProduct == 0 ? null : item.IdProduct,
							IdService = item.IdService == 0 ? null : item.IdService,
							Value = item.Value,
							Amount = item.Amount,
							InclusionDate = item.InclusionDate,
							TypeItem = item.TypeItem,
							EnableRecurrence = item.EnableRecurrence,
							RecurringAmount = item.RecurringAmount,
						};
						await saleItemsService.Save(saleItem);
					}

					if (sale.SalePayments != null && sale.SalePayments.Any())
					{
						foreach (var sp in sale.SalePayments)
						{
							sp.IdSale = s.Id;
							sp.Id = 0;
							sp.Status = SalePaymentStatus.Planned;
							await _salePaymentRepository.CreateAsync(sp);
						}
					}

					transaction.Commit();
					return s.Id;
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					throw;
				}
			}
		}

		public async Task<ResponseGeneric> ReceiveSalesOrder(int saleId, SaleDto receiveData)
		{
			using (var transaction = await repository.CreateTransactionAsync())
			{
				try
				{
					var sale = await (repository as ISaleRepository).GetSaleByCompany(saleId, receiveData.IdCompany);
					if (sale == null)
						return new ResponseGeneric { Success = false, Data = "Pedido não encontrado" };

					if (sale.Status != SaleStatus.Approved)
						return new ResponseGeneric { Success = false, Data = "Somente pedidos aprovados podem ser recebidos" };

					var existingPayments = await _salePaymentRepository.GetBySaleIdAsync(saleId);
					foreach (var ep in existingPayments)
					{
						ep.Status = SalePaymentStatus.Confirmed;
						await _salePaymentRepository.UpdateAsync(ep.Id, ep);
					}

					if (receiveData.SalePayments != null && receiveData.SalePayments.Any())
					{
						await _salePaymentRepository.DeleteBySaleIdAsync(saleId);
						foreach (var sp in receiveData.SalePayments)
						{
							sp.IdSale = saleId;
							sp.Id = 0;
							sp.Status = SalePaymentStatus.Confirmed;
							await _salePaymentRepository.CreateAsync(sp);
						}
					}

					var saleItems = await saleItemsService.GetByIdSaleAsync(saleId);
					foreach (var item in saleItems)
					{
						if (item.IdProduct != null && item.IdProduct > 0)
						{
							await _stockService.Create(new Stock
							{
								IdCompany = sale.IdCompany,
								Quantity = item.Amount,
								Date = DateTime.Now,
								IdProduct = (int)item.IdProduct,
								Reason = $"Pedido de Venda #{saleId} - Recebido",
								Type = StockType.exit,
								ReferenceId = item.Id
							});
						}
					}

					var payments = receiveData.SalePayments ?? existingPayments;
					if (payments != null && payments.Any())
					{
						var formPayments = payments.Select(sp => new FormPaymentSale
						{
							PaymentMethodId = sp.PaymentMethodId,
							Value = sp.Value,
							Installments = sp.Installments
						}).ToList();

						await GenerateFinancial(formPayments, saleId, sale.IdCompany,
							sale.IdClient, receiveData.BankAccountId, receiveData.Troco);
					}

					if (sale.IdSeller != null)
					{
						SharedCommission sharedCommission = new SharedCommission();
						foreach (var item in saleItems)
						{
							if (item.SharedCommissions != null && item.SharedCommissions.Count > 0)
							{
								sharedCommission = item.SharedCommissions.First();
							}
							await commissionService.GenerateCommission(item, sharedCommission, (int)sale.IdSeller, sale.IdCompany);
						}
					}

					sale.Status = SaleStatus.Received;
					sale.Total = receiveData.Total;
					await base.Alter(sale);

					transaction.Commit();
					return new ResponseGeneric { Success = true, Data = "Pedido recebido com sucesso" };
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					throw;
				}
			}
		}

	}
	public interface ISaleService : IBaseService<Sale>
	{
		Task<PagedResult<Sale>> GetAllPaged(Filters filter);
		Task<int> SaveWithItems(SaleDto sale);
		Task<Sale> GetByIdSale(int id);
		Task<SaleInfoResponse> GetByMonthAllSales(Filters filters);
		Task<SalesCommissionsInfo> GetByWeekAllSales(Filters filters);
		Task<List<SalesmanInfo>> GetSalesmanByWeek(int idCompany);

		Task<int> PutWithItems(SaleDto sale);
		Task<ResponseGeneric> Cancel(int saleId);
		Task<int> SaveSalesOrder(SaleDto sale);
		Task<ResponseGeneric> ReceiveSalesOrder(int saleId, SaleDto receiveData);
	}
}