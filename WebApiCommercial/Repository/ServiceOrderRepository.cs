using Microsoft.EntityFrameworkCore;
using Model;
using Model.Enums;
using Model.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    public class ServiceOrderRepository : GenericRepository<ServiceOrder>, IServiceOrderRepository
    {
        public ServiceOrderRepository(ContextBase dbContext) : base(dbContext) { }

        public async Task<PagedResult<ServiceOrder>> GetAllPaged(Filters filter)
        {
            var query = _dbContext.Set<ServiceOrder>()
                .Include(x => x.Client)
                .Include(x => x.ServiceOrderItems)
                .Include(x => x.ServiceInvoices)
                .Where(x => x.TenantId == filter.IdCompany);

            if (filter.ClientId.HasValue && filter.ClientId > 0)
                query = query.Where(x => x.ClientId == filter.ClientId.Value);

            if (!string.IsNullOrEmpty(filter.StartDate) && DateTime.TryParse(filter.StartDate, out var startDate))
                query = query.Where(x => x.OrderDate >= startDate);

            if (!string.IsNullOrEmpty(filter.EndDate) && DateTime.TryParse(filter.EndDate, out var endDate))
                query = query.Where(x => x.OrderDate <= endDate);

            if (filter.StatusNfe.HasValue)
            {
                // Using StatusNfe filter field for ServiceOrderStatus mapping
                // StatusNfe is reused here for filtering by service order status
            }

            if (!string.IsNullOrEmpty(filter.TextOption))
            {
                var search = filter.TextOption.ToLower();
                query = query.Where(x => x.Client.Name.ToLower().Contains(search)
                    || x.Id.ToString().Contains(search));
            }

            query = query.OrderByDescending(x => x.OrderDate);
            return await query.GetPagedAsync(filter.PageNumber, filter.PageSize);
        }

        public async Task<ServiceOrder> GetByIdWithDetails(int id)
        {
            return await _dbContext.Set<ServiceOrder>()
                .Include(x => x.Client)
                .Include(x => x.ServiceOrderItems)
                    .ThenInclude(x => x.ServiceProvided)
                .Include(x => x.ServiceInvoices)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<ServiceOrderItem>> GetItemsByOrderId(int orderId)
        {
            return await _dbContext.Set<ServiceOrderItem>()
                .Where(x => x.ServiceOrderId == orderId)
                .ToListAsync();
        }

        public async Task<bool> HasInvoiceEmitted(int orderId)
        {
            return await _dbContext.Set<ServiceInvoice>()
                .AnyAsync(x => x.ServiceOrderId == orderId
                    && x.Status == ServiceInvoiceStatus.Emitido);
        }

        public async Task<bool> HasPendingInvoice(int orderId)
        {
            return await _dbContext.Set<ServiceInvoice>()
                .AnyAsync(x => x.ServiceOrderId == orderId
                    && x.Status == ServiceInvoiceStatus.Pendente);
        }
    }

    public interface IServiceOrderRepository : IGenericRepository<ServiceOrder>
    {
        Task<PagedResult<ServiceOrder>> GetAllPaged(Filters filter);
        Task<ServiceOrder> GetByIdWithDetails(int id);
        Task<List<ServiceOrderItem>> GetItemsByOrderId(int orderId);
        Task<bool> HasInvoiceEmitted(int orderId);
        Task<bool> HasPendingInvoice(int orderId);
    }
}
