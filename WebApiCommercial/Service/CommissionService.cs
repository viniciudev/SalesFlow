using Model;
using Model.DTO;
using Model.Moves;
using Model.Registrations;
using Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service
{
  public class CommissionService : BaseService<Commission>, ICommissionService
  {
    private readonly IFinancialService financialService;
    private readonly ISharedCommissionService sharedCommissionService;
    public CommissionService(IGenericRepository<Commission> repository,
      IFinancialService financialService,
      ISharedCommissionService sharedCommissionService) : base(repository)
    {
      this.financialService = financialService;
      this.sharedCommissionService = sharedCommissionService;
    }

    public async Task<PagedResult<CommissionResponse>> GetAllPagedServicesCommission(Filters filter)
    {
      return await (repository as ICommissionRepository).GetAllPagedServicesCommission(filter);
    }
    public async Task<PagedResult<CommissionResponse>> GetAllPagedProductsCommission(Filters filter)
    {
      return await (repository as ICommissionRepository).GetAllPagedProductsCommission(filter);
    }
    public async Task SaveListCommission(List<Commission> commissions)
    {
      foreach (var commission in commissions)
      {
        if (commission.Id > 0)
          await base.Alter(commission);
        else
          await base.Create(commission);
      }
    }

    public async Task<string> SaveCommission(Commission commission)
    {

      if (commission.Id > 0)
        await base.Alter(commission);
      else
      {
        bool verify = await (repository as ICommissionRepository)
          .GetByIdSalesman(commission.IdSalesman,
          commission.IdProduct != null ? (int)commission.IdProduct : (int)commission.IdService,
          commission.IdProduct != null ? TypeItem.Product : TypeItem.Service);
        if (verify)
          return "Item já cadastrado!";
        await base.Create(commission);
      }
      return "success";
    }
    public async Task<Commission> CheckCommissionService(int idService, int idSeller)
    {
      return await (repository as ICommissionRepository).CheckCommissionService(idService, idSeller);
    }
    public async Task<Commission> CheckCommissionProduct(int idProduct, int idSeller)
    {
      return await (repository as ICommissionRepository).CheckCommissionProduct(idProduct, idSeller);
    }
    public async Task GenerateCommission(SaleItems item, SharedCommission sharedCommission, int idSeller, int idCompany)
    {
      Commission dataCommission = new Commission();
      if (item.TypeItem == (int)TypeItem.Service)
      {
        dataCommission = await (repository as ICommissionRepository)
          .CheckCommissionService((int)item.IdService
        , idSeller);
      }
      else
      {
        dataCommission = await (repository as ICommissionRepository)
          .CheckCommissionProduct((int)item.IdProduct
        , idSeller);
      }
      if (dataCommission != null
        && dataCommission.Status == 0
        && item.Value > 0)
      {
        int recurringAmount = item.RecurringAmount > 0 ? item.RecurringAmount : 1;
        double run = 0;
        for (int i = 0; i < recurringAmount; i++)
        {
          DateTime date = DateTime.Now.AddMonths(i);
          run += dataCommission.CommissionDay;
          DateTime dateRun = DateTime.Now.AddDays(run);
          decimal procedureValue = item.Value * item.Amount;
          decimal percentage = dataCommission.Percentage;
          decimal value = (procedureValue * (percentage / 100));
          var financial = new Financial
          {
            commission = true,
            CreationDate = DateTime.Now.Date,
            Description = $"Comissão {dataCommission.Salesman.Name} - dia: {item.InclusionDate.Date} - {item.NameItem}",
            FinancialType = FinancialType.expense,
            IdCostCenter = dataCommission.IdCostCenter,
            FinancialStatus = FinancialStatus.open,
            DueDate = dataCommission.TypeDay == TypeDay.run
         ? dateRun.Date
         : fixedDate(dataCommission.CommissionDay, date),
            Value = value,
            PaymentType = PaymentType.cash,
            IdCompany = idCompany,
            IdSalesman = idSeller,
            IdProduct = item.IdProduct == 0 ? null : item.IdProduct,
            IdService = item.IdService == 0 ? null : item.IdService,
            IdSale = item.IdSale,
            IdSaleItems = item.Id,
            Percentage = percentage,
          };
          await financialService.Create(financial);
        }
        await GenerateSharedCommission(item, sharedCommission, idCompany);
      }
    }
    private async Task GenerateSharedCommission(SaleItems item, SharedCommission sharedCommission, int idCompany)
    {
      try
      {
        if (sharedCommission.EnableSharedCommission == false && sharedCommission.Id > 0)
        {
          await sharedCommissionService.DeleteAsync(sharedCommission.Id);
        }
        else if (sharedCommission.EnableSharedCommission)
        {
          if (sharedCommission.Id > 0)
          {
            await sharedCommissionService.Alter(sharedCommission);
          }
          else
          {
            sharedCommission.IdSaleItems = item.Id;
            await sharedCommissionService.Save(sharedCommission);
          }
          int recurringAmount = sharedCommission.RecurringAmount > 0 ? sharedCommission.RecurringAmount : 1;
          double run = 0;
          for (int i = 0; i < recurringAmount; i++)
          {
            DateTime date = DateTime.Now.AddMonths(i);
            run += sharedCommission.CommissionDay;
            DateTime dateRun = DateTime.Now.AddDays(run);
            decimal procedureValue = item.Value * item.Amount;
            decimal percentage = sharedCommission.Percentage;
            decimal value = (procedureValue * (percentage / 100));
            var financial = new Financial
            {
              commission = true,
              CreationDate = DateTime.Now.Date,
              Description = $"Comissão {sharedCommission.NameSeller} - dia: {item.InclusionDate.Date} - {item.NameItem}",
              FinancialType = FinancialType.expense,
              IdCostCenter = sharedCommission.IdCostCenter,
              FinancialStatus = FinancialStatus.open,
              DueDate = sharedCommission.TypeDay == TypeDay.run
           ? dateRun.Date
           : fixedDate(sharedCommission.CommissionDay, date),
              Value = value,
              PaymentType = PaymentType.cash,
              IdCompany = idCompany,
              IdSalesman = sharedCommission.IdSalesman,
              IdProduct = item.IdProduct == 0 ? null : item.IdProduct,
              IdService = item.IdService == 0 ? null : item.IdService,
              IdSale = item.IdSale,
              IdSaleItems = item.Id,
              Percentage = percentage,
            };
            await financialService.Save(financial);
          }
        }
      }
      catch (Exception ex)
      {
        throw;
      }
    }
    public async Task AlterCommission(SaleItems item, SharedCommission sharedCommission, int idSeller, int idCompany)
    {
      Commission dataCommission = new Commission();
      List<Financial> financialData = new List<Financial>();
      if (item.TypeItem == (int)TypeItem.Service)
      {
        dataCommission = await (repository as ICommissionRepository)
          .CheckCommissionService((int)item.IdService
        , idSeller);
        financialData = await financialService
          .SearchBySaleItemsId(item.Id, TypeItem.Service, (int)item.IdService);
      }
      else
      {
        dataCommission = await (repository as ICommissionRepository)
          .CheckCommissionProduct((int)item.IdProduct
        , idSeller);
        financialData = await financialService
          .SearchBySaleItemsId(item.Id, TypeItem.Product, (int)item.IdProduct);
      }
      if (dataCommission != null
        && dataCommission.Status == 0
        && item.Value > 0)
      {
        foreach (Financial itemFinancial in financialData)
        {
          await financialService.DeleteAsync(itemFinancial.Id);
        }
        int recurringAmount = item.RecurringAmount > 0 ? item.RecurringAmount : 1;
        double run = 0;
        for (int i = 0; i < recurringAmount; i++)
        {
          DateTime date = DateTime.Now.AddMonths(i);
          run += dataCommission.CommissionDay;
          DateTime dateRun = DateTime.Now.AddDays(run);
          decimal procedureValue = item.Value * item.Amount;

          decimal percentage = dataCommission.Percentage;
          decimal value = (procedureValue * (percentage / 100));

          var financial = new Financial
          {
            commission = true,
            CreationDate = DateTime.Now.Date,
            Description = $"Comissão {dataCommission.Salesman.Name} - dia: {item.InclusionDate.Date} - {item.NameItem}",
            FinancialType = FinancialType.expense,
            IdCostCenter = dataCommission.IdCostCenter,
            FinancialStatus = FinancialStatus.open,
            DueDate = dataCommission.TypeDay == TypeDay.run
         ? dateRun.Date
         : fixedDate(dataCommission.CommissionDay, date),
            Value = value,
            PaymentType = PaymentType.cash,
            IdCompany = idCompany,
            IdSalesman = idSeller,
            IdProduct = item.IdProduct == 0 ? null : item.IdProduct,
            IdService = item.IdService == 0 ? null : item.IdService,
            IdSale = item.IdSale,
            IdSaleItems = item.Id,
            Percentage = percentage,
          };
          await financialService.Save(financial);
        }
        await GenerateSharedCommission(item, sharedCommission, idCompany);
      }
    }
    private DateTime fixedDate(int day, DateTime date)
    {
      DateTime data = date.AddDays(day - date.Day);
      if (date.Day > day)
      {
        data = data.AddMonths(1);
      }
      return data.Date;
    }
  }
  public interface ICommissionService : IBaseService<Commission>
  {
    Task<PagedResult<CommissionResponse>> GetAllPagedServicesCommission(Filters filter);
    Task<PagedResult<CommissionResponse>> GetAllPagedProductsCommission(Filters filter);
    Task SaveListCommission(List<Commission> commissions);
    Task GenerateCommission(SaleItems saleItems, SharedCommission sharedCommission, int idSeller, int idCompany);
    Task AlterCommission(SaleItems item, SharedCommission sharedCommission, int idSeller, int idCompany);
    Task<string> SaveCommission(Commission commission);
  }
}
