using Model;
using Model.DTO;
using Model.Moves;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourNamespace.DTOs;

namespace Service
{
	public class FinancialService : BaseService<Financial>, IFinancialService
	{
		private readonly ICostCenterRepository _costCenterRepository;
		private readonly IFinancialResourceRepository _financialResourceRepository;
		private readonly IFinancialPaymentMethodRepository _financialPaymentMethodRepository;
		public FinancialService(IGenericRepository<Financial> repository,
				ICostCenterRepository costCenterRepository,
				IFinancialResourceRepository financialResourceRepository,
				IFinancialPaymentMethodRepository financialPaymentMethodRepository) : base(repository)
		{
			_costCenterRepository = costCenterRepository;
			_financialResourceRepository = financialResourceRepository;
			_financialPaymentMethodRepository = financialPaymentMethodRepository;
		}
		public async Task<List<Financial>> SearchBySaleItemsId(int id, TypeItem typeItem, int idItem)
		{
			return await (repository as IFinancialRepository).SearchBySaleItemsId(id, typeItem, idItem);
		}
		public async Task<PagedResult<CommissionFinancialResponse>> GetPagedByFilter(Filters filters)
		{
			return await (repository as IFinancialRepository).GetPagedByFilter(filters);
		}
		public async Task<CommissionInfoResponse> GetByMonthAllCommission(Filters filters)
		{
			return await (repository as IFinancialRepository).GetByMonthAllCommission(filters);
		}
		public async Task DeleteFinancial(int id)
		{
			try
			{
				Financial financialData = await (repository as IFinancialRepository).GetById(id);
				if (financialData != null)
				{
					if (financialData.FinancialPaymentMethods != null)
					{
						foreach (var item in financialData.FinancialPaymentMethods)
						{
							await _financialPaymentMethodRepository.DeleteAsync(item.Id);
						}

					}
				}
					await base.DeleteAsync(id);
			}
			catch (System.Exception ex)
			{

				throw;
			}

		}
		public async Task<List<Financial>> GetByIdCompany(Filters filters)
		{
			return await (repository as IFinancialRepository).GetByIdCompany(filters);
		}
		public async Task AlterFinancial(FinancialRequest financial)
		{
			Financial financialData = await (repository as IFinancialRepository).GetById(financial.Id);
			if (financialData != null)
			{
				if (financialData.FinancialPaymentMethods != null)
				{
					foreach (var item in financialData.FinancialPaymentMethods)
					{
						await _financialPaymentMethodRepository.DeleteAsync(item.Id);
					}

				}
				List<FinancialPaymentMethod> financialPaymentMethod = new();
				foreach (var item in financial.PaymentMethods)
				{
					financialPaymentMethod.Add(new FinancialPaymentMethod
					{
						PaymentMethodId = item.PaymentMethodId,
						FinancialId = financialData.Id,
						Amount = item.Value,
						//      Installments = item.Installments
					});
				}
				;
				financialData.FinancialPaymentMethods = financialPaymentMethod;
				financialData.Value = financial.Value;
				financialData.FinancialType = financial.FinancialType;
				financialData.Description = financial.Description;
				financialData.DueDate = financial.DueDate;
				financialData.FinancialStatus = financial.FinancialStatus;
				financialData.BankAccountId = financial.BankAccountId;
				financialData.SettlementDate = financial.SettlementDate; // Novo
				financialData.InterestValue = financial.InterestValue;     // Novo
				financialData.FineValue = financial.FineValue;             // Novo
				financialData.SettledValue = financial.SettledValue;       // Novo
				await base.Alter(financialData);
			}
		}
		public async Task<List<int>> CreateFinancial(FinancialInstallmentRequest financial)
		{
			try
			{
				ValidateFinancialRequest(financial);

				var listCostCenter = await _costCenterRepository.GetByIdCompany(financial.IdCompany);
				int? costCenterId = listCostCenter.FirstOrDefault()?.Id;

				int installments = financial.NumberOfInstallments <= 0 ? 1 : financial.NumberOfInstallments;
				var ids = new List<int>();

				// Registro simples (sem parcelamento) - comportamento existente
				if (installments == 1)
				{
					Financial fin = BuildFinancialRecord(financial, financial.Value, financial.DueDate,
						financial.Description, costCenterId, installmentIndex: -1, installments: 1);
					await base.Save(fin);
					ids.Add(fin.Id);
					return ids;
				}

				// Parcelamento: divide o valor em partes iguais e gera um registro por parcela
				decimal installmentValue = Math.Round(financial.Value / installments, 2);
				decimal remainingValue = financial.Value;
				bool hasManualDueDates = financial.InstallmentDueDates != null
					&& financial.InstallmentDueDates.Count == installments;

				for (int i = 0; i < installments; i++)
				{
					// A última parcela absorve a diferença de arredondamento
					decimal currentValue = (i == installments - 1)
						? Math.Round(remainingValue, 2)
						: installmentValue;
					remainingValue -= currentValue;

					// Vencimento manual (se informado) ou sequencial por intervalo de dias
					DateTime dueDate = hasManualDueDates
						? financial.InstallmentDueDates[i]
						: financial.DueDate.AddDays(i * financial.InstallmentIntervalDays);

					string description = $"Parcela {i + 1}/{installments} - {financial.Description}";

					Financial fin = BuildFinancialRecord(financial, currentValue, dueDate,
						description, costCenterId, installmentIndex: i, installments: installments);
					await base.Save(fin);
					ids.Add(fin.Id);
				}

				return ids;
			}
			catch (System.Exception ex)
			{
				throw;
			}
		}

		/// <summary>
		/// Valida os dados do request de financeiro/parcelamento.
		/// </summary>
		private void ValidateFinancialRequest(FinancialInstallmentRequest financial)
		{
			if (financial.Value <= 0)
				throw new Exception("O valor total deve ser maior que zero.");
			if (financial.NumberOfInstallments < 1)
				throw new Exception("O número de parcelas deve ser maior que zero.");
			if (financial.InstallmentIntervalDays < 1)
				throw new Exception("O intervalo entre parcelas deve ser maior que zero.");
			if (financial.InstallmentDueDates != null &&
			    financial.InstallmentDueDates.Count >0 &&
			    financial.InstallmentDueDates.Count != financial.NumberOfInstallments)
				throw new Exception("O número de datas de vencimento deve ser igual ao número de parcelas.");
			if (financial.PaymentMethods == null || !financial.PaymentMethods.Any())
				throw new Exception("Adicione pelo menos uma forma de pagamento.");
		}

		/// <summary>
		/// Monta um registro financeiro a partir do request, aplicando o valor e a
		/// data de vencimento da parcela. Quando parcelado, cada parcela recebe
		/// uma FinancialPaymentMethod com o valor rateado (a última parcela absorve
		/// a diferença de arredondamento).
		/// </summary>
		private Financial BuildFinancialRecord(
			FinancialInstallmentRequest financial,
			decimal value,
			DateTime dueDate,
			string description,
			int? costCenterId,
			int installmentIndex,
			int installments)
		{
			Financial fin = new Financial
			{
				BankAccountId = financial.BankAccountId,
				FinancialStatus = financial.FinancialStatus,
				FinancialType = financial.FinancialType,
				CreationDate = financial.CreationDate,
				DueDate = dueDate,
				Description = description,
				Origin = financial.Origin,
				IdCompany = (int)financial.IdCompany,
				IdCostCenter = costCenterId,
				Value = value,
				IdClient = financial.ClientId,
				SettlementDate = financial.SettlementDate,
				InterestValue = financial.InterestValue,
				FineValue = financial.FineValue,
				SettledValue = financial.SettledValue,
			};

			List<FinancialPaymentMethod> financialPaymentMethod = new();
			foreach (var item in financial.PaymentMethods)
			{
				decimal amount;
				if (installments > 1)
				{
					decimal pmInstallment = Math.Round(item.Value / installments, 2);
					amount = (installmentIndex == installments - 1)
						? Math.Round(item.Value - (pmInstallment * (installments - 1)), 2)
						: pmInstallment;
				}
				else
				{
					amount = item.Value;
				}

				financialPaymentMethod.Add(new FinancialPaymentMethod
				{
					PaymentMethodId = item.PaymentMethodId,
					FinancialId = fin.Id,
					Amount = amount,
					Installments = 1
				});
			}
			fin.FinancialPaymentMethods = financialPaymentMethod;

			return fin;
		}
		public async Task<List<Financial>> GetByIdSaleAsync(int id)
		{
			return await (repository as IFinancialRepository).GetByIdSaleAsync(id);
		}
		public async Task<PagedResultWithTotals> GetPaged(Filters filters)
		{
			return await (repository as IFinancialRepository).GetPaged(filters);
		}
		public async Task<PagedResult<FinancialResponse>> GetPagedByIdClient(Filters filters)
		{
			return await (repository as IFinancialRepository).GetPagedByIdClient(filters);
		}
		public async Task AlterFinancialStatus(Financial financial)
		{
			try
			{
				Financial financialData = await base.GetByIdAsync(financial.Id);
				financialData.FinancialStatus = financial.FinancialStatus;
				financialData.SettlementDate = DateTime.Now.ToString("yyyy-MM-dd");
				await base.Alter(financialData);
			}
			catch (System.Exception ex)
			{

				throw;
			}
		}
		public async Task CreateRenegotiationAsync(RenegotiationRequestDto request)
		{
			await GenerateFinancial(request);
		}
		private async Task GenerateFinancial(RenegotiationRequestDto request)
		{
			try
			{


				//cria nova parcela
				var listCostCenter = await _costCenterRepository.GetByIdCompany(request.IdCompany);
				for (int i = 0; i < request.NumberOfInstallments; i++)
				{
					Financial financial = new Financial();
					financial.Id = 0;
					financial.FinancialStatus =  FinancialStatus.pending;
					financial.FinancialType = FinancialType.recipe;
					financial.Origin = OriginFinancial.renegotiation;

					financial.CreationDate = DateTime.Now;
					financial.DueDate = i == 0 ? request.NewDueDate : request.NewDueDate.AddMonths(i);
					financial.IdCompany = request.IdCompany;
					financial.Description = request.Description;
					financial.IdCostCenter = listCostCenter.FirstOrDefault()?.Id;
					financial.IdClient = request.ClientId;

					financial.Value = (request.NewValue / request.NumberOfInstallments);
					List<FinancialPaymentMethod> financialPaymentMethod = new();
					foreach (var item in request.PaymentMethods)
					{
						financialPaymentMethod.Add(new FinancialPaymentMethod
						{
							PaymentMethodId = item.Id,
							FinancialId = financial.Id,
							//Amount = item.Amount,
							//      Installments = item.Installments
						});
					}
					financial.FinancialPaymentMethods = financialPaymentMethod;
					await base.Create(financial);

					foreach (var id in request.OriginalInstallments)
					{
						await _financialResourceRepository.CreateAsync(
						 new FinancialResources
						 {
							 IdRefOrigin = id,
							 IdNewFinancial = financial.Id
						 });
					}
				}

				//muda o status pra renegociado
				foreach (var id in request.OriginalInstallments)
				{
					await AlterFinancialStatus(new Financial
					{
						Id = id,
						FinancialStatus = FinancialStatus.renegotiated,
					});
				}


			}
			catch (Exception ex)
			{

				throw;
			}
		}
		public async Task<List<Financial>> GetByIdPurchaseAsync(int id)
		{
			return await (repository as IFinancialRepository).GetByIdPurchaseAsync(id);
		}
	}
	public interface IFinancialService : IBaseService<Financial>
	{
		Task<List<Financial>> SearchBySaleItemsId(int id, TypeItem typeItem, int idItem);
		Task<PagedResult<CommissionFinancialResponse>> GetPagedByFilter(Filters filters);
		Task DeleteFinancial(int id);
		Task<CommissionInfoResponse> GetByMonthAllCommission(Filters filters);
		Task<List<Financial>> GetByIdCompany(Filters filters);
		Task AlterFinancial(FinancialRequest financial);
		Task<List<Financial>> GetByIdSaleAsync(int id);
		Task<List<int>> CreateFinancial(FinancialInstallmentRequest financial);
		Task<PagedResultWithTotals> GetPaged(Filters filters);
		Task AlterFinancialStatus(Financial financial);
		Task<PagedResult<FinancialResponse>> GetPagedByIdClient(Filters filters);
		Task CreateRenegotiationAsync(RenegotiationRequestDto request);
		Task<List<Financial>> GetByIdPurchaseAsync(int id);
	}
}
