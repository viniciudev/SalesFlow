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
    public class SaleService : BaseService<Sale>, ISaleService
    {
        private readonly ISaleItemsService saleItemsService;
        private readonly ICommissionService commissionService;

        public SaleService(IGenericRepository<Sale> repository,
          ISaleItemsService saleItemsService,
          ICommissionService commissionService) : base(repository)
        {
            this.saleItemsService = saleItemsService;
            this.commissionService = commissionService;
        }

        public async Task<PagedResult<Sale>> GetAllPaged(Filters filters)
        {
            return await (repository as ISaleRepository).GetAllPaged(filters);
        }
        public async Task<int> SaveWithItems(Sale sale)
        {
            using (var transaction = await repository.CreateTransactionAsync())
            {
                try
                {
                    Sale s = new Sale
                    {
                        //Id = sale.Id,
                        IdClient = sale.IdClient,
                        IdCompany = sale.IdCompany,
                        IdSeller = sale.IdSeller == 0 ? null : sale.IdSeller,
                        ReleaseDate = sale.ReleaseDate,
                        SaleDate = sale.SaleDate,
                        Total=sale.Total
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

                        if (item.SharedCommissions!=null &&item.SharedCommissions.Count > 0)
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

    }
    public interface ISaleService : IBaseService<Sale>
    {
        Task<PagedResult<Sale>> GetAllPaged(Filters filter);
        Task<int> SaveWithItems(Sale sale);
        Task<Sale> GetByIdSale(int id);
        Task<SaleInfoResponse> GetByMonthAllSales(Filters filters);
        Task<SalesCommissionsInfo> GetByWeekAllSales(Filters filters);
        Task<List<SalesmanInfo>> GetSalesmanByWeek(int idCompany);
    }
}
