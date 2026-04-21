using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO;
using Model.DTO.Financial;
using Model.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
	public class FinancialRepository : GenericRepository<Financial>, IFinancialRepository
	{
		public FinancialRepository(ContextBase dbContext) : base(dbContext)
		{
		}
		public async Task<List<Financial>> SearchBySaleItemsId(int id, TypeItem typeItem, int idItem)
		{
			var data = await _dbContext.Set<Financial>().Where(
				x => x.IdSaleItems == id
				&& (typeItem == TypeItem.Product && x.IdProduct == idItem
				|| typeItem == TypeItem.Service && x.IdService == idItem)
				).AsNoTracking().ToListAsync();
			return data;
		}
		public async Task<PagedResult<CommissionFinancialResponse>> GetPagedByFilter(Filters filters)
		{
			try
			{
				var data = await (from fin in _dbContext.Set<Financial>()
									.Include(x => x.Sale).ThenInclude(x => x.Client)
									.Include(x => x.Product)
									.Include(x => x.ServiceProvided)
													where (fin.IdSalesman == 0 || fin.IdSalesman == filters.IdSeller)
													&& (fin.CreationDate >= filters.SaleDate.Date
															&& fin.CreationDate <= filters.SaleDateFinal.Date)
													select new CommissionFinancialResponse
													{
														IdFinancial = fin.Id,
														ReleaseDate = fin.CreationDate,
														DateSale = fin.Sale.ReleaseDate,
														Item = fin.Product != null ? fin.Product.Name : fin.ServiceProvided.Name,
														Percentage = fin.Percentage,
														Value = fin.Value,
														DueDate = fin.DueDate,
														Origin = fin.Sale.Client.Name
													}).AsNoTracking()
									 .GetPagedAsync<CommissionFinancialResponse>(filters.PageNumber, filters.PageSize);

				return data;
			}
			catch (System.Exception ex)
			{
				throw;
			}
		}
		public async Task<List<Financial>> GetByIdCompany(Filters filters)
		{
			try
			{
				var data = await (from fin in _dbContext.Set<Financial>()

													where (fin.IdCompany == filters.IdCompany)
													&& (fin.Origin == OriginFinancial.financial)
													&& (fin.CreationDate >= filters.SaleDate.Date
															&& fin.CreationDate <= filters.SaleDateFinal.Date)
													select new Financial
													{
														Id = fin.Id,
														Value = fin.Value,
														DueDate = fin.DueDate,
														Description = fin.Description,
														FinancialType = fin.FinancialType,
														PaymentType = fin.PaymentType,
														CreationDate = fin.CreationDate,
													}).AsNoTracking()
									 .GetPagedAsync<Financial>(filters.PageNumber, filters.PageSize);

				return data.Results.ToList();
			}
			catch (System.Exception ex)
			{
				throw;
			}
		}
		public async Task<PagedResultWithTotals> GetPaged(Filters filters)
		{
			try
			{
				// Query base
				var baseQuery = from fin in _dbContext.Set<Financial>()
												.Include(x => x.Sale).ThenInclude(x => x.Client)
												.Include(x => x.Product)
												.Include(x => x.ServiceProvided)
												.Include(x => x.Client)
												.Include(x => x.FinancialPaymentMethods)
												.ThenInclude(x => x.PaymentMethod)
												.Include(x => x.BankAccount)

												where fin.IdCompany == filters.IdCompany

												// Filtro de texto
												&& (string.IsNullOrEmpty(filters.TextOption)
																|| fin.Description.Contains(filters.TextOption)
																|| fin.Client.Name.Contains(filters.TextOption))

												// Filtro de status
												&& (filters.FinancialStatus == null
																|| fin.FinancialStatus == filters.FinancialStatus)

												// Filtro de tipo
												&& (fin.FinancialType == filters.FinancialType)

												// Filtro de período
												&& (string.IsNullOrEmpty(filters.StartDate)
																|| fin.DueDate >= DateTime.Parse(filters.StartDate))
												&& (string.IsNullOrEmpty(filters.EndDate)
																|| fin.DueDate <= DateTime.Parse(filters.EndDate).AddDays(1).AddSeconds(-1))

												// Filtro de cliente
												&& (filters.ClientId == null
																|| fin.IdClient == filters.ClientId)

												// Filtro de forma de pagamento
												&& (filters.PaymentMethodId == null
																|| fin.FinancialPaymentMethods
															 .Any(x=>x.PaymentMethod.Id==filters.PaymentMethodId))

												// Filtro de conta bancária
												&& (filters.BankAccountId == null
																|| fin.BankAccountId == filters.BankAccountId)

												select fin;

				// Calcular totais em uma única consulta
				var totalsQuery = from fin in baseQuery
													group fin by fin.FinancialType into g
													select new
													{
														FinancialType = g.Key,
														TotalPending = g.Where(x => x.FinancialStatus == FinancialStatus.pending
																										 || x.FinancialStatus == FinancialStatus.renegotiated)
																							.Sum(x => x.Value),
														TotalPaid = g.Where(x => x.FinancialStatus == FinancialStatus.paid)
																					.Sum(x => x.Value)
													};

				var totalsResult = await totalsQuery.ToListAsync();

				var totals = new Totals();

				foreach (var item in totalsResult)
				{
					if (item.FinancialType == FinancialType.recipe) // Receitas
					{
						totals.TotalReceivable = item.TotalPending;
						totals.TotalReceived = item.TotalPaid;
						totals.TotalGeneralReceivable = item.TotalPending + item.TotalPaid;
					}
					else if (item.FinancialType == FinancialType.expense) // Despesas
					{
						totals.TotalPayable = item.TotalPending;
						totals.TotalPaid = item.TotalPaid;
						totals.TotalGeneralPayable = item.TotalPending + item.TotalPaid;
					}
				}

				// Query para paginação
				var pagedQuery = from fin in baseQuery
												 orderby fin.DueDate
												 select new FinancialResponse
												 {
													 Id = fin.Id,
													 CreationDate = fin.CreationDate,
													 Value = fin.Value,
													 DueDate = fin.DueDate,
													 Origin = fin.Origin,
													 FinancialStatus = fin.FinancialStatus,
													 PaymentMethods= fin.FinancialPaymentMethods.Select(x =>
													 new PaymentsDto { PaymentMethodId=x.PaymentMethodId, 
														 PaymentMethodName=x.PaymentMethod.Name,
													 Value=x.Amount
													 }).ToList(),
													 BankAccountId = fin.BankAccountId,
													 Description = fin.Description,
													 FinancialType = fin.FinancialType,
													 IdCompany = fin.IdCompany,
													 ClientName = fin.Client != null ? fin.Client.Name : null,
													 ClientId = fin.IdClient,
													 FinancialResourcesResponseList = _dbContext.Set<FinancialResources>()
																 .Where(fr => fr.IdNewFinancial == fin.Id)
																 .Join(_dbContext.Set<Financial>(),
																			 fr => fr.IdRefOrigin,
																			 f => f.Id,
																			 (fr, f) => new { fr, f })
																 .Select(x => new FinancialResourcesResponse
																 {
																	 Id = x.f.Id,
																	 Description = x.f.Description,
																	 Value = x.f.Value
																 })
																 .ToList(),
													 SettlementDate =  fin.SettlementDate,
													 FineValue=fin.FineValue,
													 SettledValue=fin.SettledValue,
													 InterestValue=fin.InterestValue
												 };

				var pagedResult = await pagedQuery.AsNoTracking()
																					.WithCaseInsensitive()
																					.GetPagedAsync<FinancialResponse>(filters.PageNumber, filters.PageSize);

				return new PagedResultWithTotals
				{
					PagedResult = pagedResult,
					Totals = totals
				};
			}
			catch (System.Exception ex)
			{
				// Log do erro aqui
				throw;
			}
		}
		public async Task<CommissionInfoResponse> GetByMonthAllCommission(Filters filters)
		{
			try
			{
				CommissionInfoResponse clientInfoResponse = new CommissionInfoResponse { AmountMonth = new List<int>(), CommissionAmount = 0 };
				List<Financial> data = await _dbContext.Set<Financial>()
					.Where(x => x.IdCompany == filters.IdCompany
					&& x.CreationDate.Year == DateTime.Now.Year
					&& x.commission == true)
					.AsNoTracking().ToListAsync();

				var grupo1 = data.GroupBy(c => c.CreationDate.Month)
																	.Select(g => new { Key = g.Key, Itens = g.ToList() }).ToList();

				for (int i = 1; i < 13; i++)
				{
					var teste = grupo1.FirstOrDefault(x => x.Key == i);
					if (teste != null && teste.Key > 0)
						clientInfoResponse.AmountMonth.Add(teste.Itens.Count);
					else
						clientInfoResponse.AmountMonth.Add(0);
				}
				clientInfoResponse.CommissionAmount = data.Count();
				return clientInfoResponse;
			}
			catch (System.Exception ex)
			{

				throw;
			}
		}
		public async Task<List<Financial>> GetByIdSaleAsync(int id)
		{
			var data = await _dbContext.Set<Financial>()
				.Include(x => x.FinancialPaymentMethods)
					.Where(x => x.IdSale == id)
					.AsNoTracking()
					.ToListAsync();
			return data;
		}
		public async Task<PagedResult<Financial>> GetPagedByIdClient(Filters filters)
		{

			try
			{
				var data = await (from fin in _dbContext.Set<Financial>()
														 .Include(x => x.FinancialPaymentMethods).ThenInclude(x => x.PaymentMethod)
													where (fin.IdCompany == filters.IdCompany)
													&& (fin.IdClient == filters.IdClient)
												 && (fin.FinancialStatus == FinancialStatus.pending)
												 && (fin.FinancialType == FinancialType.recipe)
													select new Financial
													{
														Id = fin.Id,
														Value = fin.Value,
														DueDate = fin.DueDate,
														Description = fin.Description,
														FinancialType = fin.FinancialType,
														PaymentMethodName = fin.FinancialPaymentMethods.Select(x => x.PaymentMethod.Name).ToList(),
														CreationDate = fin.CreationDate,
														FinancialStatus = fin.FinancialStatus,

													}).AsNoTracking()
									 .GetPagedAsync(filters.PageNumber, filters.PageSize);

				return data;
			}
			catch (System.Exception ex)
			{
				throw;
			}

		}
		public async Task<Financial> GetById(int id)
		{
			var data = await _dbContext.Set<Financial>()
				.Include(x => x.FinancialPaymentMethods)
					.Where(x => x.Id == id)
					.AsNoTracking()
					.FirstOrDefaultAsync();
			return data;
		}
	}
	public interface IFinancialRepository : IGenericRepository<Financial>
	{
		Task<List<Financial>> SearchBySaleItemsId(int id, TypeItem typeItem, int idItem);
		Task<PagedResult<CommissionFinancialResponse>> GetPagedByFilter(Filters filters);
		Task<CommissionInfoResponse> GetByMonthAllCommission(Filters filters);
		Task<List<Financial>> GetByIdCompany(Filters filters);
		Task<List<Financial>> GetByIdSaleAsync(int id);
		Task<PagedResultWithTotals> GetPaged(Filters filters);
		Task<PagedResult<Financial>> GetPagedByIdClient(Filters filters);
		Task<Financial> GetById(int id);
	}
	public class PagedResultWithTotals
	{
		public PagedResult<FinancialResponse> PagedResult { get; set; }
		public Totals Totals { get; set; }
	}

	public class Totals
	{
		// Totais para receitas (FinancialType = 0)
		public decimal TotalReceivable { get; set; }      // Total a receber (pending + renegotiated)
		public decimal TotalReceived { get; set; }         // Total recebido (paid)

		// Totais para despesas (FinancialType = 1)
		public decimal TotalPayable { get; set; }          // Total a pagar (pending + renegotiated)
		public decimal TotalPaid { get; set; }              // Total pago (paid)

		// Totais gerais (opcional)
		public decimal TotalGeneralReceivable { get; set; } // Total geral de receitas
		public decimal TotalGeneralPayable { get; set; }    // Total geral de despesas
	}
}
