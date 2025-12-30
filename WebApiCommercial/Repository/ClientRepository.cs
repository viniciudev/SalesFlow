using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    public class ClientRepository : GenericRepository<Client>, IClientRepository
    {
        public ClientRepository(ContextBase dbContext) : base(dbContext)
        {
        }

        public async Task<PagedResult<Client>> GetAllPaged(Filters clientFilter)
        {
            var paged = await base._dbContext.Set<Client>()
               .Where(x => (x.IdCompany == clientFilter.IdCompany)
               && (String.IsNullOrEmpty(clientFilter.cellPhoneOption)
               || (x.CellPhone == clientFilter.cellPhoneOption)) &&
               (clientFilter.SelectOption == FilterType.Name
               && (string.IsNullOrEmpty(clientFilter.TextOption)
               || x.Name.Contains(clientFilter.TextOption))))
               .AsNoTracking()
               .WithCaseInsensitive()
               .OrderByDescending(x=>x.Id)
               .GetPagedAsync<Client>(clientFilter.PageNumber, clientFilter.PageSize);
            return paged;
        }
        public async Task<List<Client>> GetByName(Filters clientFilter)
        {
            var data = await _dbContext.Set<Client>()
              .Where(x => x.IdCompany == clientFilter.IdCompany
              && x.Name.Contains(clientFilter.TextOption)).AsNoTracking().ToListAsync();
            return data;
        }
        public async Task<List<Client>> GetAllList(Filters clientFilter)
        {
            var data = await (from cli in _dbContext.Set<Client>()
                              where cli.IdCompany == clientFilter.IdCompany
                              select new Client
                              {
                                  Name = cli.Name,
                                  Id = cli.Id,
                              }
              ).AsNoTracking().WithCaseInsensitive().ToListAsync();
            return data;
        }
        public async Task<ClientInfoResponse> GetByMonthAllClients(Filters filters)
        {
            try
            {
                ClientInfoResponse clientInfoResponse = new ClientInfoResponse { AmountMonth = new List<int>(), ClientAmount = 0 };
                List<Client> data = await _dbContext.Set<Client>()
                  .Where(x => x.IdCompany == filters.IdCompany
                  && x.CreatDate.Year == DateTime.Now.Year)
                  .AsNoTracking()
                  .WithCaseInsensitive().ToListAsync();

                var grupo1 = data.GroupBy(c => c.CreatDate.Month)
                                          .Select(g => new { Key = g.Key, Itens = g.ToList() }).ToList();

                for (int i = 1; i < 13; i++)
                {
                    var teste = grupo1.FirstOrDefault(x => x.Key == i);
                    if (teste != null && teste.Key > 0)
                        clientInfoResponse.AmountMonth.Add(teste.Itens.Count);
                    else
                        clientInfoResponse.AmountMonth.Add(0);
                }
                clientInfoResponse.ClientAmount = data.Count();
                return clientInfoResponse;
            }
            catch (System.Exception ex)
            {

                throw;
            }
        }
        public async Task<List<Client>> GetByFilter(Filters filter)
        {
            List<Client> data = await _dbContext.Set<Client>()
              .Where(x =>( x.IdCompany == filter.IdCompany)
              && (string.IsNullOrEmpty(filter.TextOption) || x.Name.Contains(filter.TextOption)))
              .AsNoTracking().WithCaseInsensitive()
              .ToListAsync();
            return data;
        }
        public async Task<MonthlyClientsComparisonResult> GetMonthlyClientsWithComparisonByIdCompany(int idCompany)
        {
            var currentDate = DateTime.Now;
            var currentMonth = currentDate.Month;
            var currentYear = currentDate.Year;

            var previousMonthDate = currentDate.AddMonths(-1);
            var previousMonth = previousMonthDate.Month;
            var previousMonthYear = previousMonthDate.Year;

            // Usar um período de 60 dias para garantir os dois meses
            var startDate = new DateTime(previousMonthYear, previousMonth, 1);
            var endDate = new DateTime(currentYear, currentMonth, 1).AddMonths(1).AddDays(-1);

            var monthlyCounts = await _dbContext.Set<Client>()
                .Where(c => c.IdCompany == idCompany
                        && c.CreatDate >= startDate
                        && c.CreatDate <= endDate)
                .GroupBy(c => new { c.CreatDate.Month, c.CreatDate.Year })
                .Select(g => new
                {
                    g.Key.Month,
                    g.Key.Year,
                    ClientCount = g.Count()
                })
                .AsNoTracking()
                .ToListAsync();

            var currentMonthCount = monthlyCounts
                .FirstOrDefault(m => m.Month == currentMonth && m.Year == currentYear)?
                .ClientCount ?? 0;

            var previousMonthCount = monthlyCounts
                .FirstOrDefault(m => m.Month == previousMonth && m.Year == previousMonthYear)?
                .ClientCount ?? 0;

            // Calcular percentual
            decimal percentage = 0;
            if (previousMonthCount > 0)
            {
                percentage = ((currentMonthCount - (decimal)previousMonthCount) / previousMonthCount) * 100;
            }
            else if (currentMonthCount > 0)
            {
                percentage = 100;
            }

            return new MonthlyClientsComparisonResult
            {
                CurrentMonth = new MonthlyClientsComparisonResult.MonthData
                {
                    Month = currentMonth,
                    Year = currentYear,
                    ClientCount = currentMonthCount
                },
                PreviousMonth = new MonthlyClientsComparisonResult.MonthData
                {
                    Month = previousMonth,
                    Year = previousMonthYear,
                    ClientCount = previousMonthCount
                },
                PercentageChange = Math.Round(percentage, 2),
                IsIncrease = percentage >= 0,
                TotalComparison = currentMonthCount - previousMonthCount
            };
        }
    }
    public interface IClientRepository : IGenericRepository<Client>
    {
        Task<ClientInfoResponse> GetByMonthAllClients(Filters filters);
        Task<PagedResult<Client>> GetAllPaged(Filters clientFilter);
        Task<List<Client>> GetByName(Filters clientFilter);
        Task<List<Client>> GetAllList(Filters clientFilter);
        Task<List<Client>> GetByFilter(Filters filter);
        Task<MonthlyClientsComparisonResult> GetMonthlyClientsWithComparisonByIdCompany(int idCompany);
    }
}
