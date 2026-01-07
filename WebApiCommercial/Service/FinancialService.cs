using Model;
using Model.DTO;
using Model.Moves;
using Model.Registrations;
using Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service
{
    public class FinancialService : BaseService<Financial>, IFinancialService
    {
        private readonly ICostCenterRepository _costCenterRepository;
        public FinancialService(IGenericRepository<Financial> repository, ICostCenterRepository costCenterRepository) : base(repository)
        {
            _costCenterRepository = costCenterRepository;
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
    }
}
