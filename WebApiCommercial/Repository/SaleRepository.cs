using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO;
using Model.Moves;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Model.DTO.MonthlySalesComparisonResult;

namespace Repository
{
    public class SaleRepository : GenericRepository<Sale>, ISaleRepository
    {
        public SaleRepository(ContextBase dbContext) : base(dbContext)
        {
        }

        public async Task<PagedResult<Sale>> GetAllPaged(Filters filters)
        {
            var paged = await (from sale in base._dbContext.Set<Sale>()
                               .Include(x => x.Salesman)
                               .Include(x => x.SaleItems)
                               .Include(x => x.Client)
                               .Include(x => x.Financials)
                               where sale.IdCompany == filters.IdCompany
                               && (filters.IdClient == 0 || sale.IdClient == filters.IdClient)
                               && (filters.IdSeller == 0 || sale.IdSeller == filters.IdSeller)
                               && (sale.SaleDate >= filters.SaleDate.Date
                               && sale.SaleDate <= filters.SaleDateFinal.Date.
                               AddHours(23).AddMinutes(59).AddSeconds(59))
                               && (string.IsNullOrEmpty(filters.TextOption) || sale.Client.Name.Contains(filters.TextOption))
                               orderby sale.SaleDate descending
                               select new Sale
                               {
                                   Id = sale.Id,
                                   ReleaseDate = sale.ReleaseDate,
                                   SaleDate = sale.SaleDate,
                                   NameSeller = sale.Salesman.Name,
                                   ValueSale = sale.SaleItems.Sum(x => x.Value * x.Amount),
                                   Total = sale.Total,

                                   SaleItems = sale.SaleItems.Select(x => new SaleItems
                                   {
                                       Id = x.Id,
                                       IdProduct = x.Product.Id,
                                       ProductName = x.Product.Name,
                                       Amount = x.Amount,
                                       Value = x.Value,
                                       IdSale = x.IdSale
                                   }).ToList(),
                                   NameClient = sale.Client.Name,
                                   IdClient = sale.IdClient,
                                   Financials = sale.Financials
                               })
                               .AsNoTracking()
                               .GetPagedAsync<Sale>(filters.PageNumber, filters.PageSize);
            return paged;
        }
        public async Task<Sale> GetByIdSale(int id)
        {
            var paged = await (from sale in base._dbContext.Set<Sale>().
                               Include(x => x.SaleItems).ThenInclude(x => x.Product)
                               .Include(x => x.SaleItems).ThenInclude(x => x.ServiceProvided)
                                .Include(x => x.SaleItems).ThenInclude(x => x.SharedCommissions)
                               join cli in base._dbContext.Set<Client>()
                               on sale.IdClient equals cli.Id
                               join saller in base._dbContext.Set<Salesman>()
                               on sale.IdSeller equals saller.Id into left
                               from saller in left.DefaultIfEmpty()
                               where sale.Id == id
                               select
                               new Sale
                               {
                                   Id = sale.Id,
                                   IdCompany = sale.IdCompany,
                                   IdClient = sale.IdClient,
                                   NameClient = cli.Name,
                                   IdSeller = sale.IdSeller,
                                   NameSeller = saller.Name,
                                   ReleaseDate = sale.ReleaseDate,
                                   SaleDate = sale.SaleDate,
                                   SaleItems = FormatSaleItems(sale.SaleItems)
                               })
                               .AsNoTracking()
                               .FirstOrDefaultAsync();
            return paged;
        }
        static List<SaleItems> FormatSaleItems(ICollection<SaleItems> saleItems)
        {
            List<SaleItems> result = new List<SaleItems>();
            foreach (SaleItems item in saleItems)
            {
                result.Add(
                 new SaleItems
                 {
                     Id = item.Id,
                     Amount = item.Amount,
                     Value = item.Value,
                     InclusionDate = item.InclusionDate,
                     NameItem = item.Product == null ? item.ServiceProvided.Name : item.Product.Name,
                     IdProduct = item.IdProduct,
                     IdService = item.IdService,
                     TypeItem = item.TypeItem,
                     EnableRecurrence = item.EnableRecurrence,
                     RecurringAmount = item.RecurringAmount,
                     SharedCommissions = item.SharedCommissions,
                 });
            }
            return result;
        }
        public async Task<SaleInfoResponse> GetByMonthAllSales(Filters filters)
        {
            try
            {
                SaleInfoResponse saleInfoResponse = new SaleInfoResponse { AmountMonth = new List<int>(), SalesAmount = 0 };
                List<Sale> data = await _dbContext.Set<Sale>()
                  .Where(x => x.IdCompany == filters.IdCompany
                  && x.SaleDate.Year == DateTime.Now.Year)
                  .AsNoTracking().ToListAsync();

                var grupo1 = data.GroupBy(c => c.SaleDate.Month)
                                          .Select(g => new { Key = g.Key, Itens = g.ToList() }).ToList();

                for (int i = 1; i < 13; i++)
                {
                    var teste = grupo1.FirstOrDefault(x => x.Key == i);
                    if (teste != null && teste.Key > 0)
                        saleInfoResponse.AmountMonth.Add(teste.Itens.Count);
                    else
                        saleInfoResponse.AmountMonth.Add(0);
                }
                saleInfoResponse.SalesAmount = data.Count();
                return saleInfoResponse;
            }
            catch (System.Exception ex)
            {

                throw;
            }
        }

        public async Task<SalesCommissionsInfo> GetByWeekAllSales(Filters filters)
        {
            try
            {
                var today = DateTime.Today;
                var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);

                SalesCommissionsInfo saleInfoResponse = new SalesCommissionsInfo
                {
                    AmountOfCommissionsAndSales = new List<AmountOfCommissionsAndSales>(),
                    CommissiontAmountWeek = 0,
                    SalesAmountWeek = 0
                };
                List<Sale> data = await _dbContext.Set<Sale>()
                  .Include(x => x.Financials)
                  .Where(x => x.IdCompany == filters.IdCompany
                  && (x.SaleDate >= thisWeekStart && x.SaleDate <= thisWeekEnd)
                  && x.SaleDate.Year == DateTime.Now.Year)
                  .AsNoTracking().ToListAsync();

                var grupo1 = data.GroupBy(c => c.SaleDate.Date.DayOfWeek)
                                          .Select(g => new { Key = g.Key, Itens = g.ToList() }).ToList();

                for (DateTime i = thisWeekStart; i <= thisWeekEnd; i = i.AddDays(1))
                {
                    var teste = grupo1.FirstOrDefault(x => x.Key == i.DayOfWeek);
                    if (teste != null && teste.Key > 0)
                    {
                        var amount = new AmountOfCommissionsAndSales
                        {
                            Commissions = teste.Itens.First().Financials.Count,
                            Sales = teste.Itens.Count,
                            Day = i.DayOfWeek.ToString()
                        };
                        saleInfoResponse.AmountOfCommissionsAndSales.Add(amount);
                    }
                    else
                    {
                        var amount = new AmountOfCommissionsAndSales
                        {
                            Commissions = 0,
                            Sales = 0,
                            Day = i.DayOfWeek.ToString()
                        };
                        saleInfoResponse.AmountOfCommissionsAndSales.Add(amount);
                    }

                }
                saleInfoResponse.SalesAmountWeek = data.Count();
                saleInfoResponse.CommissiontAmountWeek = data.Sum(x => x.Financials.Count);
                return saleInfoResponse;
            }
            catch (System.Exception ex)
            {

                throw;
            }
        }

        public async Task<List<SalesmanInfo>> GetSalesmanByWeek(int idCompany)
        {
            var today = DateTime.Today;
            var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
            var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);

            var data = await _dbContext.Set<Sale>()
              .Include(x => x.Salesman)
            .Where(x => x.IdCompany == idCompany
             && (x.SaleDate >= thisWeekStart && x.SaleDate <= thisWeekEnd)
                && x.SaleDate.Year == DateTime.Now.Year)
            .GroupBy(x => x.Salesman.Name).Select(x => new SalesmanInfo { Name = x.Key, AmountOfSales = x.ToList().Count })
            .OrderByDescending(x => x.AmountOfSales).ToListAsync();
            return data;
        }
        public async Task<MonthlySalesComparisonResult> GetMonthlySalesWithComparisonByIdCompany(int id)
        {
            var currentDate = DateTime.Now;
            var currentMonth = currentDate.Month;
            var currentYear = currentDate.Year;

            var previousMonth = currentDate.AddMonths(-1);
            var previousMonthNumber = previousMonth.Month;
            var previousMonthYear = previousMonth.Year;

            // Buscar todas as vendas dos dois meses em uma única consulta
            var sales = await _dbContext.Set<Sale>()
                .Where(s => s.IdCompany == id
                        && (
                            (s.SaleDate.Month == currentMonth && s.SaleDate.Year == currentYear) ||
                            (s.SaleDate.Month == previousMonthNumber && s.SaleDate.Year == previousMonthYear)
                        ))
                .Select(s => new
                {
                    s.Total,
                    s.SaleDate.Month,
                    s.SaleDate.Year
                })
                .AsNoTracking()
                .ToListAsync();

            // Separar e calcular os totais
            var currentMonthSales = sales.Where(s => s.Month == currentMonth && s.Year == currentYear);
            var previousMonthSales = sales.Where(s => s.Month == previousMonthNumber && s.Year == previousMonthYear);

            var currentMonthTotal = currentMonthSales.Sum(s => s.Total);
            var previousMonthTotal = previousMonthSales.Sum(s => s.Total);

            // Calcular percentual
            decimal percentage = 0;
            if (previousMonthTotal > 0)
            {
                percentage = ((currentMonthTotal - previousMonthTotal) / previousMonthTotal) * 100;
            }
            else if (currentMonthTotal > 0)
            {
                percentage = 100;
            }

            return new MonthlySalesComparisonResult
            {
                CurrentMonth = new MonthData
                {
                    Month = currentMonth,
                    Year = currentYear,
                    TotalSales = currentMonthTotal,
                    SalesCount = currentMonthSales.Count()
                },
                PreviousMonth = new MonthData
                {
                    Month = previousMonthNumber,
                    Year = previousMonthYear,
                    TotalSales = previousMonthTotal,
                    SalesCount = previousMonthSales.Count()
                },
                PercentageChange = Math.Round(percentage, 2),
                IsIncrease = percentage >= 0
            };
        }
    }

    public interface ISaleRepository : IGenericRepository<Sale>
    {
        Task<PagedResult<Sale>> GetAllPaged(Filters filters);
        Task<Sale> GetByIdSale(int id);
        Task<SaleInfoResponse> GetByMonthAllSales(Filters filters);
        Task<SalesCommissionsInfo> GetByWeekAllSales(Filters filters);
        Task<List<SalesmanInfo>> GetSalesmanByWeek(int idCompany);
        Task<MonthlySalesComparisonResult> GetMonthlySalesWithComparisonByIdCompany(int id);
    }
}

