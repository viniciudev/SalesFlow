using Microsoft.EntityFrameworkCore;
using Model;
using Model.Enums;
using Model.Moves;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    public class ServiceInvoiceRepository : GenericRepository<ServiceInvoice>, IServiceInvoiceRepository
    {
        public ServiceInvoiceRepository(ContextBase dbContext) : base(dbContext) { }

        public async Task<PagedResult<ServiceInvoice>> GetAllPaged(Filters filter)
        {
            var query = _dbContext.Set<ServiceInvoice>()
                .Include(x => x.Client)
                .Include(x => x.ServiceOrder)
                .Include(x => x.ServiceInvoiceItems)
                .Where(x => x.TenantId == filter.IdCompany);

            if (filter.ClientId.HasValue && filter.ClientId > 0)
                query = query.Where(x => x.ClientId == filter.ClientId.Value);

            if (!string.IsNullOrEmpty(filter.StartDate) && System.DateTime.TryParse(filter.StartDate, out var startDate))
                query = query.Where(x => x.DataCompetencia >= startDate);

            if (!string.IsNullOrEmpty(filter.EndDate) && System.DateTime.TryParse(filter.EndDate, out var endDate))
                query = query.Where(x => x.DataCompetencia <= endDate);

            if (filter.StatusNfe.HasValue)
            {
                // Map StatusNfe to ServiceInvoiceStatus
                // 0=Pendente, 1=Emitido, 2=Cancelado
            }

            if (!string.IsNullOrEmpty(filter.TextOption))
            {
                var search = filter.TextOption.ToLower();
                query = query.Where(x => x.Client.Name.ToLower().Contains(search)
                    || x.NumeroDPS.ToString().Contains(search)
                    || x.IdDPS.ToLower().Contains(search));
            }

            query = query.OrderByDescending(x => x.CreatedAt);
            return await query.GetPagedAsync(filter.PageNumber, filter.PageSize);
        }

        public async Task<ServiceInvoice> GetByIdWithDetails(int id)
        {
            return await _dbContext.Set<ServiceInvoice>()
                .Include(x => x.Client)
                .Include(x => x.ServiceOrder)
                .Include(x => x.ServiceInvoiceItems)
                    .ThenInclude(x => x.ServiceProvided)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<int> GetNextInvoiceNumber(int tenantId)
        {
            var config = await _dbContext.Set<Model.Registrations.FiscalConfiguration>()
                .FirstOrDefaultAsync(x => x.CompanyId == tenantId);

            if (config == null)
                return 1;

            return config.LastInvoiceNumber + 1;
        }

        public async Task<List<ServiceInvoice>> GetInvoicesByOrderId(int orderId)
        {
            return await _dbContext.Set<ServiceInvoice>()
                .Include(x => x.ServiceInvoiceItems)
                .Where(x => x.ServiceOrderId == orderId)
                .ToListAsync();
        }

        public async Task<bool> HasEmittedInvoicesForService(int orderId, int serviceProvidedId)
        {
            return await _dbContext.Set<ServiceInvoiceItem>()
                .AnyAsync(x => x.ServiceProvidedId == serviceProvidedId
                    && x.ServiceInvoice.ServiceOrderId == orderId
                    && x.ServiceInvoice.Status == ServiceInvoiceStatus.Emitido);
        }

        public async Task<List<ServiceInvoiceItem>> GetInvoiceItems(int invoiceId)
        {
            return await _dbContext.Set<ServiceInvoiceItem>()
                .Where(x => x.ServiceInvoiceId == invoiceId)
                .ToListAsync();
        }

        public async Task AddInvoiceItems(List<ServiceInvoiceItem> items)
        {
            await _dbContext.Set<ServiceInvoiceItem>().AddRangeAsync(items);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateInvoiceItems(int invoiceId, List<ServiceInvoiceItem> items)
        {
            var existing = await _dbContext.Set<ServiceInvoiceItem>()
                .Where(x => x.ServiceInvoiceId == invoiceId)
                .ToListAsync();

            _dbContext.Set<ServiceInvoiceItem>().RemoveRange(existing);
            await _dbContext.Set<ServiceInvoiceItem>().AddRangeAsync(items);
            await _dbContext.SaveChangesAsync();
        }

        public async Task IncrementInvoiceNumber(int tenantId)
        {
            var config = await _dbContext.Set<Model.Registrations.FiscalConfiguration>()
                .FirstOrDefaultAsync(x => x.CompanyId == tenantId);

            if (config != null)
            {
                config.LastInvoiceNumber++;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Model.Registrations.FiscalConfiguration> GetFiscalConfiguration(int tenantId)
        {
            return await _dbContext.Set<Model.Registrations.FiscalConfiguration>()
                .FirstOrDefaultAsync(x => x.CompanyId == tenantId);
        }
    }

    public interface IServiceInvoiceRepository : IGenericRepository<ServiceInvoice>
    {
        Task<PagedResult<ServiceInvoice>> GetAllPaged(Filters filter);
        Task<ServiceInvoice> GetByIdWithDetails(int id);
        Task<int> GetNextInvoiceNumber(int tenantId);
        Task<List<ServiceInvoice>> GetInvoicesByOrderId(int orderId);
        Task<bool> HasEmittedInvoicesForService(int orderId, int serviceProvidedId);
        Task<List<ServiceInvoiceItem>> GetInvoiceItems(int invoiceId);
        Task AddInvoiceItems(List<ServiceInvoiceItem> items);
        Task UpdateInvoiceItems(int invoiceId, List<ServiceInvoiceItem> items);
        Task IncrementInvoiceNumber(int tenantId);
        Task<Model.Registrations.FiscalConfiguration> GetFiscalConfiguration(int tenantId);
    }
}
