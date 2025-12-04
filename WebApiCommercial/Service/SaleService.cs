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
        private readonly IStockService _stockService;
        private readonly ICostCenterRepository _costCenterRepository;
        private readonly IFinancialService _financialService;

        public SaleService(IGenericRepository<Sale> repository,
          ISaleItemsService saleItemsService,
          ICommissionService commissionService,
          IStockService stockService,
          ICostCenterRepository costCenterRepository,
          IFinancialService financialService) : base(repository)
        {
            this.saleItemsService = saleItemsService;
            this.commissionService = commissionService;
            _stockService = stockService;
            _costCenterRepository = costCenterRepository;
            _financialService = financialService;
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
                        IdClient = sale.IdClient,
                        IdCompany = sale.IdCompany,
                        IdSeller = sale.IdSeller == 0 ? null : sale.IdSeller,
                        ReleaseDate = sale.ReleaseDate,
                        SaleDate = sale.SaleDate,
                        Total = sale.Total
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
                            ReferenceId=data.Id
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
                    await GenerateFinancial(sale.Financials, s.Id, sale.IdCompany);

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
        private async Task GenerateFinancial(ICollection<Financial> financials,int IdSale,int IdCompany)
        {
            var listCostCenter=await _costCenterRepository.GetByIdCompany(IdCompany);
            foreach (var item in financials)
            {
                item.Id = 0;
                item.FinancialStatus = FinancialStatus.paid;
                item.FinancialType = FinancialType.recipe;
                item.Origin = OriginFinancial.financial;
                item.IdSale = IdSale;
                item.CreationDate = DateTime.Now;
                item.DueDate = DateTime.Now;
                item.IdCompany = IdCompany;
                item.Description = $"Venda no dia:{DateTime.Now}";
                item.IdCostCenter= listCostCenter.FirstOrDefault()?.Id;
                try
                {
                    await _financialService.Create(item);
                }
                catch (Exception ex)
                {

                    throw;
                }
                
            }
        }
        public async Task<int> PutWithItems(Sale sale)
        {
            using (var transaction = await repository.CreateTransactionAsync())
            {
                try
                {
                    Sale s = new Sale
                    {
                        Id = sale.Id,
                        IdClient = sale.IdClient,
                        IdCompany = sale.IdCompany,
                        IdSeller = sale.IdSeller == 0 ? null : sale.IdSeller,
                        ReleaseDate = sale.ReleaseDate,
                        SaleDate = sale.SaleDate,
                        Total = sale.Total
                    };
                    await base.Alter(s);

                    SaleItems data = new SaleItems();
                    SharedCommission sharedCommission = new SharedCommission();
                    foreach (var item in sale.SaleItems)
                    {
                        //deletar itens pra incluir tudo depois
                        if(item.Id>0)
                        await saleItemsService.DeleteAsync(item.Id);
                        //voltar estoque
                        Stock stock = await _stockService.GetByReferenceIdAsync(item.Id);
                        //deletar para inserir com os novos itens
                        if (stock != null)
                        {
                            await _stockService.DeleteAsync(stock.Id);
                        }
                     
                    }
                    //deletar financeiros
                     var fin = await _financialService.GetByIdSaleAsync(sale.Id);
                    foreach (var item in fin)
                    {
                        await _financialService.DeleteAsync(item.Id);
                    }
                    //gerar novos financeiros
                    await GenerateFinancial(sale.Financials, s.Id, sale.IdCompany);
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
                            ReferenceId = data.Id,
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
        Task<int> PutWithItems(Sale sale);
    }
}
