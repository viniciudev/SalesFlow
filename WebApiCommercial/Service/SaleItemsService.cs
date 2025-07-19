using Model;
using Model.Moves;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service
{
  public class SaleItemsService : BaseService<SaleItems>, ISaleItemsService
  {
    private readonly ICommissionService commissionService;
    private readonly IFinancialService financialService;
    private readonly ISharedCommissionService sharedCommissionService;
    public SaleItemsService(IGenericRepository<SaleItems> repository,
      ICommissionService commissionService,
      IFinancialService financialService,
     ISharedCommissionService sharedCommissionService) : base(repository)
    {
      this.commissionService = commissionService;
      this.financialService = financialService;
      this.sharedCommissionService= sharedCommissionService;
    }

    public async Task<PagedResult<SaleItems>> GetPaged(Filters filters)
    {
      return await (repository as ISaleItemsRepository).GetPaged(filters);
    }
    public async Task SaveSaleItems(SaleItems saleItems, int tenantid)
    {
      using (var transaction = await repository.CreateTransactionAsync())
      {
        try
        {
          SharedCommission sharedCommission = new SharedCommission();
          SaleItems data = new SaleItems
          {
            IdSale = saleItems.IdSale,
            IdProduct = saleItems.IdProduct == 0 ? null : saleItems.IdProduct,
            IdService = saleItems.IdService == 0 ? null : saleItems.IdService,
            Value = saleItems.Value,
            Amount = saleItems.Amount,
            InclusionDate = saleItems.InclusionDate,
            TypeItem = saleItems.TypeItem,
            EnableRecurrence= saleItems.EnableRecurrence,
            RecurringAmount= saleItems.RecurringAmount,
          };
          await base.Create(data);

          if (saleItems.SharedCommissions.Count > 0)
            sharedCommission = new SharedCommission
          {
            Id = saleItems.SharedCommissions.First().Id,
            CommissionDay = saleItems.SharedCommissions.First().CommissionDay,
            IdCostCenter = saleItems.SharedCommissions.First().IdCostCenter,
            IdSaleItems = saleItems.Id,
            Percentage = saleItems.SharedCommissions.First().Percentage,
            EnableSharedCommission = saleItems.SharedCommissions.First().EnableSharedCommission,
            IdSalesman = saleItems.SharedCommissions.First().IdSalesman,
            NameSeller = saleItems.SharedCommissions.First().NameSeller,
            RecurringAmount = saleItems.SharedCommissions.First().RecurringAmount,
            TypeDay = saleItems.SharedCommissions.First().TypeDay,
          };
          if (saleItems.IdSeller > 0)
            await commissionService.GenerateCommission(data, sharedCommission, (int)saleItems.IdSeller, tenantid);
          transaction.Commit();
        }
        catch (Exception ex) { 
          transaction.Rollback(); 
        }
      }
    }
    public async Task AlterSaleItems(SaleItems saleItems, int tenantid)
    {
      using (var transaction = await repository.CreateTransactionAsync())
      {
        try
        {
          SharedCommission sharedCommission = new SharedCommission();
          await base.Alter(new SaleItems
          {
            Id = saleItems.Id,
            IdSale = saleItems.IdSale,
            IdProduct = saleItems.IdProduct == 0 ? null : saleItems.IdProduct,
            IdService = saleItems.IdService == 0 ? null : saleItems.IdService,
            Value = saleItems.Value,
            Amount = saleItems.Amount,
            InclusionDate = saleItems.InclusionDate,
            TypeItem = saleItems.TypeItem,
            EnableRecurrence = saleItems.EnableRecurrence,
            RecurringAmount = saleItems.RecurringAmount
          });

          if(saleItems.SharedCommissions.Count>0)
          sharedCommission = new SharedCommission
          {
            Id = saleItems.SharedCommissions.First().Id,
            CommissionDay = saleItems.SharedCommissions.First().CommissionDay,
            IdCostCenter = saleItems.SharedCommissions.First().IdCostCenter,
            IdSaleItems = saleItems.Id,
            Percentage = saleItems.SharedCommissions.First().Percentage,
            EnableSharedCommission = saleItems.SharedCommissions.First().EnableSharedCommission,
            IdSalesman = saleItems.SharedCommissions.First().IdSalesman,
            NameSeller = saleItems.SharedCommissions.First().NameSeller,
            RecurringAmount = saleItems.SharedCommissions.First().RecurringAmount,
            TypeDay = saleItems.SharedCommissions.First().TypeDay,
          };

          if (saleItems.IdSeller > 0)
            await commissionService.AlterCommission(saleItems, sharedCommission,(int)saleItems.IdSeller, tenantid);
          transaction.Commit();
        }
        catch (Exception ex)
        {
          transaction.Rollback();
          throw;
        }
      }
    }
    public async Task Delete(int id)
    {
      try
      {
        SaleItems saleItems = await base.GetByIdAsync(id);
        if (saleItems != null)
        {
          List<Financial> financial = new List<Financial>();
          if (saleItems.IdProduct > 0)
            financial = await financialService.SearchBySaleItemsId(saleItems.Id, TypeItem.Product, (int)saleItems.IdProduct);
          else
            financial = await financialService.SearchBySaleItemsId(saleItems.Id, TypeItem.Service, (int)saleItems.IdService);
          foreach (Financial item in financial)
          {
            await financialService.DeleteAsync(item.Id);
          }
          SharedCommission sharedCommission = await sharedCommissionService.GetByIdSaleItems(saleItems.Id);
          if (sharedCommission != null)
          {
            await sharedCommissionService.DeleteAsync(sharedCommission.Id);
          }
          await base.DeleteAsync(saleItems.Id);
        }
      }
      catch (Exception ex)
      {
        throw;
      }
    }
  }
  public interface ISaleItemsService : IBaseService<SaleItems>
  {
    Task<PagedResult<SaleItems>> GetPaged(Filters filters);
    Task SaveSaleItems(SaleItems saleItems, int tenantid);
    Task AlterSaleItems(SaleItems saleItems, int tenantid);
    Task Delete(int id);
  }
}
