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
               .Where(x => x.IdCompany == clientFilter.IdCompany
               && (String.IsNullOrEmpty(clientFilter.cellPhoneOption)
               || (x.CellPhone == clientFilter.cellPhoneOption)) &&
               (clientFilter.SelectOption == FilterType.Name
               && (string.IsNullOrEmpty(clientFilter.TextOption)
               || x.Name.Contains(clientFilter.TextOption))))
               .AsNoTracking()
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
              ).AsNoTracking().ToListAsync();
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
                  .AsNoTracking().ToListAsync();

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
              .Where(x => x.IdCompany == filter.IdCompany
              && string.IsNullOrEmpty(filter.TextOption) || x.Name.Contains(filter.TextOption))
              .AsNoTracking().ToListAsync();
            return data;
        }
    }
    public interface IClientRepository : IGenericRepository<Client>
    {
        Task<ClientInfoResponse> GetByMonthAllClients(Filters filters);
        Task<PagedResult<Client>> GetAllPaged(Filters clientFilter);
        Task<List<Client>> GetByName(Filters clientFilter);
        Task<List<Client>> GetAllList(Filters clientFilter);
        Task<List<Client>> GetByFilter(Filters filter);
    }
}
