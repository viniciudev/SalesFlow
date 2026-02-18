using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{

    public class BankAccountRepository : GenericRepository<BankAccount>, IBankAccountRepository
    {
        public BankAccountRepository(ContextBase dbContext) : base(dbContext)
        {

        }
        public async Task<PagedResult<BankAccountDto>> GetPagedAsync(BankAccountFilterDto filter)
        {
            var query = _dbContext.Set<BankAccount>().AsQueryable();

            query = query.Where(x => x.IdCompany == filter.IdCompany);
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(x =>
                    x.BankName.ToLower().Contains(term) ||
                    x.BankCode.ToLower().Contains(term) ||
                    x.AgencyNumber.Contains(term) ||
                    x.AccountNumber.Contains(term) ||
                    x.HolderName.ToLower().Contains(term) ||
                    x.HolderDocument.Contains(term));
            }

            var items = await query.OrderByDescending(x => x.Id)
                .Select(x => new BankAccountDto
                {
                    Id = x.Id,
                    BankCode = x.BankCode,
                    BankName = x.BankName,
                    AgencyNumber = x.AgencyNumber,
                    AgencyDigit = x.AgencyDigit,
                    AccountNumber = x.AccountNumber,
                    AccountDigit = x.AccountDigit,
                    AccountType = x.AccountType.ToString(),
                    HolderName = x.HolderName,
                    HolderDocument = x.HolderDocument,
                    HolderType = x.HolderType.ToString(),
                    PixKey = x.PixKey,
                    IsActive = x.IsActive,
                })
                                   .GetPagedAsync(filter.Page ?? 1, filter.PageSize ?? 10);
            return items;


        }
        public async Task<IEnumerable<BankAccountDto>> GetByFilterAsync(int idcompany,string searchTerm)
        {
            var query = _dbContext.Set<BankAccount>().Where(x=>x.IdCompany==idcompany). AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(x =>
                    x.BankName.ToLower().Contains(term) ||
                    x.BankCode.ToLower().Contains(term) ||
                    x.AgencyNumber.Contains(term) ||
                    x.AccountNumber.Contains(term) ||
                    x.HolderName.ToLower().Contains(term) ||
                    x.HolderDocument.Contains(term));
            }
            var items = await query.

                OrderByDescending(x => x.Id)
                .Select(x => new BankAccountDto
                {
                    Id = x.Id,
                    BankCode = x.BankCode,
                    BankName = x.BankName,
                    AgencyNumber = x.AgencyNumber,
                    AgencyDigit = x.AgencyDigit,
                    AccountNumber = x.AccountNumber,
                    AccountDigit = x.AccountDigit,
                    AccountType = x.AccountType.ToString(),
                    HolderName = x.HolderName,
                    HolderDocument = x.HolderDocument,
                    HolderType = x.HolderType.ToString(),
                    PixKey = x.PixKey,
                    IsActive = x.IsActive,
                })
                .ToListAsync();
            return items;
        }
    }
    public interface IBankAccountRepository : IGenericRepository<BankAccount>
    {
        Task<IEnumerable<BankAccountDto>> GetByFilterAsync(int idcompany, string searchTerm);
        Task<PagedResult<BankAccountDto>> GetPagedAsync(BankAccountFilterDto filter);
    }
}
