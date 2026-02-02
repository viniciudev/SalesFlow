using Model;
using Model.DTO;
using Model.Moves;
using Model.Registrations;
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
       private readonly IFinancialResourceRepository  _financialResourceRepository;
        public FinancialService(IGenericRepository<Financial> repository,
            ICostCenterRepository costCenterRepository,
            IFinancialResourceRepository financialResourceRepository) : base(repository)
        {
            _costCenterRepository = costCenterRepository;
            _financialResourceRepository = financialResourceRepository;
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
        public async Task AlterFinancial(Financial financial)
        {
            Financial financialData = await base.GetByIdAsync(financial.Id);
            financialData.Value = financial.Value;
            financialData.FinancialType = financial.FinancialType;
            financialData.Description = financial.Description;
            financialData.DueDate = financial.DueDate;
            financialData.FinancialStatus = financial.FinancialStatus;
            financialData.PaymentType = financial.PaymentType;
            await base.Alter(financialData);
        }
        public async Task<bool> CreateFinancial(FinancialRequest financial)
        {
            try
            {
            var listCostCenter = await _costCenterRepository.GetByIdCompany(financial.IdCompany);
            Financial fin = new Financial
            {
                FinancialStatus = financial.FinancialStatus,
                FinancialType = financial.FinancialType,
                PaymentType = financial.PaymentType,
                CreationDate = financial.CreationDate,
                DueDate = financial.DueDate,
                Description = financial.Description,
                Origin = financial.Origin,
                IdCompany=(int)financial.IdCompany,
                IdCostCenter = listCostCenter.FirstOrDefault()?.Id,
                Value=financial.Value,
                IdClient=financial.ClientId
            };
            await base.Save(fin);
                return true;
            }
            catch (System.Exception ex)
            {

               return false;
            }

        }
        public async Task<List<Financial>> GetByIdSaleAsync(int id)
        {
            return await (repository as IFinancialRepository).GetByIdSaleAsync(id);
        }
        public async Task<PagedResult<FinancialResponse>> GetPaged(Filters filters)
        {
            return await (repository as IFinancialRepository).GetPaged(filters);
        }
        public async Task<PagedResult<Financial>> GetPagedByIdClient(Filters filters)
        {
            return await (repository as IFinancialRepository).GetPagedByIdClient(filters);
        }
        public async Task AlterFinancialStatus(Financial financial)
        {
            try
            {
                Financial financialData = await base.GetByIdAsync(financial.Id);
                financialData.FinancialStatus = financial.FinancialStatus;
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
                financial.FinancialStatus =i==0? FinancialStatus.paid:FinancialStatus.pending;
                financial.FinancialType = FinancialType.recipe;
                financial.Origin = OriginFinancial.renegotiation;
                    financial.PaymentType = PaymentType.cash;
                financial.CreationDate = DateTime.Now;
                financial.DueDate = i==0?request.NewDueDate:request.NewDueDate.AddMonths(i);
                financial.IdCompany = request.IdCompany;
                financial.Description = request.Description;
                financial.IdCostCenter = listCostCenter.FirstOrDefault()?.Id;
                financial.IdClient = request.ClientId;
             
                financial.Value = (request.NewValue / request.NumberOfInstallments);
               
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
    }
    public interface IFinancialService : IBaseService<Financial>
    {
        Task<List<Financial>> SearchBySaleItemsId(int id, TypeItem typeItem, int idItem);
        Task<PagedResult<CommissionFinancialResponse>> GetPagedByFilter(Filters filters);
        Task DeleteFinancial(int id);
        Task<CommissionInfoResponse> GetByMonthAllCommission(Filters filters);
        Task<List<Financial>> GetByIdCompany(Filters filters);
        Task AlterFinancial(Financial financial);
        Task<List<Financial>> GetByIdSaleAsync(int id);
        Task<bool> CreateFinancial(FinancialRequest financial);
        Task<PagedResult<FinancialResponse>> GetPaged(Filters filters);
        Task AlterFinancialStatus(Financial financial);
        Task<PagedResult<Financial>> GetPagedByIdClient(Filters filters);
        Task CreateRenegotiationAsync(RenegotiationRequestDto request);
    }
}
