using Model.DTO;
using Model;
using Model.Enums;
using Model.Moves;
using Repository;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;

namespace Service
{
  public class FinancialService : BaseService<Financial>, IFinancialService
  {
    public FinancialService(IGenericRepository<Financial> repository) : base(repository)
    {
    }
    public async Task<List<Financial>> SearchBySaleItemsId(int id, TypeItem typeItem, int idItem)
    {
      return await (repository as IFinancialRepository).SearchBySaleItemsId(id,  typeItem, idItem);
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
      return await(repository as IFinancialRepository).GetByIdCompany(filters);
    }
    public async Task AlterFinancial(Financial financial)
    {
      Financial financialData = await base.GetByIdAsync(financial.Id);
      financialData.Value=financial.Value;
      financialData.FinancialType= financial.FinancialType;
      financialData.Description=financial.Description;
      financialData.DueDate=financial.DueDate;
      financialData.PaymentType= financial.PaymentType;
      await base.Alter(financialData);
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
  }
}
