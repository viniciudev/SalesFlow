using Microsoft.EntityFrameworkCore.Storage;
using Model;
using Model.DTO;
using Model.Enums;
using Model.Moves;
using Repository;
using Service.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service
{
    public class ServiceOrderService : BaseService<ServiceOrder>, IServiceOrderService
    {
        private readonly IServiceOrderRepository _orderRepo;
        private readonly IServiceInvoiceRepository _invoiceRepo;

        public ServiceOrderService(
            IGenericRepository<ServiceOrder> repository,
            IServiceOrderRepository orderRepo,
            IServiceInvoiceRepository invoiceRepo) : base(repository)
        {
            _orderRepo = orderRepo;
            _invoiceRepo = invoiceRepo;
        }

        public async Task<PagedResult<ServiceOrderResponse>> GetAllPaged(Filters filter)
        {
            var paged = await _orderRepo.GetAllPaged(filter);
            var responses = new PagedResult<ServiceOrderResponse>
            {
                CurrentPage = paged.CurrentPage,
                PageCount = paged.PageCount,
                PageSize = paged.PageSize,
                RowCount = paged.RowCount,
                Results = paged.Results.Select(MapToResponse).ToList()
            };
            return responses;
        }

        public async Task<ServiceOrderResponse> GetByIdAsync(int id)
        {
            var entity = await _orderRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("Ordem de serviço não encontrada.");
            return MapToResponse(entity);
        }

        public async Task<ServiceOrderResponse> CreateAsync(ServiceOrderCreateRequest request, Guid userId)
        {
            ValidateCreate(request);

            var entity = new ServiceOrder
            {
                TenantId = request.TenantId,
                ClientId = request.ClientId,
                OrderDate = request.OrderDate,
                Status = ServiceOrderStatus.Aberta,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = userId,
                ServiceOrderItems = new List<ServiceOrderItem>()
            };

            foreach (var item in request.Items)
            {
                entity.ServiceOrderItems.Add(new ServiceOrderItem
                {
                    ServiceProvidedId = item.ServiceProvidedId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice,
                    CreatedAt = DateTime.UtcNow
                });
            }

            entity.TotalValue = entity.ServiceOrderItems.Sum(x => x.TotalPrice);

            await repository.CreateAsync(entity);

            return MapToResponse(entity);
        }

        public async Task<ServiceOrderResponse> UpdateAsync(int id, ServiceOrderUpdateRequest request, Guid userId)
        {
            var entity = await _orderRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("Ordem de serviço não encontrada.");

            if (entity.Status == ServiceOrderStatus.Concluida || entity.Status == ServiceOrderStatus.Cancelada)
                throw new DomainException("Não é possível editar uma ordem de serviço concluída ou cancelada.");

            entity.ClientId = request.ClientId;
            entity.OrderDate = request.OrderDate;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId;

            var existingItems = entity.ServiceOrderItems.ToList();
            foreach (var existing in existingItems)
            {
                var updatedItem = request.Items.FirstOrDefault(x => x.ServiceProvidedId == existing.ServiceProvidedId);
                if (updatedItem != null)
                {
                    existing.Quantity = updatedItem.Quantity;
                    existing.UnitPrice = updatedItem.UnitPrice;
                    existing.TotalPrice = updatedItem.Quantity * updatedItem.UnitPrice;
                }
            }

            var existingServiceIds = existingItems.Select(x => x.ServiceProvidedId).ToList();
            foreach (var item in request.Items.Where(x => !existingServiceIds.Contains(x.ServiceProvidedId)))
            {
                entity.ServiceOrderItems.Add(new ServiceOrderItem
                {
                    ServiceProvidedId = item.ServiceProvidedId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var requestServiceIds = request.Items.Select(x => x.ServiceProvidedId).ToList();
            var itemsToRemove = existingItems.Where(x => !requestServiceIds.Contains(x.ServiceProvidedId)).ToList();
            foreach (var item in itemsToRemove)
            {
                entity.ServiceOrderItems.Remove(item);
            }

            entity.TotalValue = entity.ServiceOrderItems.Sum(x => x.TotalPrice);

            await base.Alter(entity);

            return MapToResponse(entity);
        }

        public async Task<ServiceOrderResponse> ChangeStatusAsync(int id, ServiceOrderStatus newStatus, Guid userId)
        {
            var entity = await _orderRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("Ordem de serviço não encontrada.");

            ValidateStatusTransition(entity.Status, newStatus);

            if (newStatus == ServiceOrderStatus.Concluida)
            {
                var hasEmittedInvoice = await _orderRepo.HasInvoiceEmitted(id);
                if (!hasEmittedInvoice)
                    throw new DomainException("A ordem de serviço só pode ser concluída se houver ao menos uma NFSe emitida.");
            }

            entity.Status = newStatus;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId;

            if (newStatus == ServiceOrderStatus.Concluida)
                entity.ConcludedAt = DateTime.UtcNow;

            await base.Alter(entity);

            return MapToResponse(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _orderRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("Ordem de serviço não encontrada.");

            if (entity.Status == ServiceOrderStatus.Concluida)
                throw new DomainException("Não é possível cancelar uma ordem de serviço concluída.");

            entity.Status = ServiceOrderStatus.Cancelada;
            entity.UpdatedAt = DateTime.UtcNow;
            await base.Alter(entity);
        }

        public async Task<List<AvailableServiceResponse>> GetAvailableServices(int orderId)
        {
            var order = await _orderRepo.GetByIdWithDetails(orderId);
            if (order == null)
                throw new DomainException("Ordem de serviço não encontrada.");

            var items = order.ServiceOrderItems.Select(i => new AvailableServiceResponse
            {
                ServiceProvidedId = i.ServiceProvidedId,
                Name = i.ServiceProvided?.Name ?? "",
                Description = i.ServiceProvided?.Description ?? "",
                Value = i.UnitPrice,
                LocationCode = i.ServiceProvided?.LocationCode ?? "",
                NationalTaxCode = i.ServiceProvided?.NationalTaxCode ?? ""
            }).ToList();

            return items;
        }

        private void ValidateCreate(ServiceOrderCreateRequest request)
        {
            var errors = new List<string>();

            if (request.ClientId <= 0)
                errors.Add("Cliente é obrigatório.");

            if (request.Items == null || request.Items.Count == 0)
                errors.Add("Pelo menos um serviço é obrigatório.");

            if (request.Items != null)
            {
                var duplicateIds = request.Items
                    .GroupBy(x => x.ServiceProvidedId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);

                if (duplicateIds.Any())
                    errors.Add("Não é permitido serviços duplicados na mesma ordem.");
            }

            if (errors.Count > 0)
                throw new DomainException(string.Join("; ", errors));
        }

        private void ValidateStatusTransition(ServiceOrderStatus current, ServiceOrderStatus next)
        {
            if (current == ServiceOrderStatus.Cancelada)
                throw new DomainException("Não é possível alterar o status de uma ordem cancelada.");

            if (current == ServiceOrderStatus.Concluida && next != ServiceOrderStatus.Cancelada)
                throw new DomainException("Ordem concluída só pode ser cancelada.");

            var validTransitions = new Dictionary<ServiceOrderStatus, List<ServiceOrderStatus>>
            {
                { ServiceOrderStatus.Aberta, new List<ServiceOrderStatus> { ServiceOrderStatus.EmAndamento, ServiceOrderStatus.Cancelada } },
                { ServiceOrderStatus.EmAndamento, new List<ServiceOrderStatus> { ServiceOrderStatus.Concluida, ServiceOrderStatus.Cancelada } },
                { ServiceOrderStatus.Concluida, new List<ServiceOrderStatus> { ServiceOrderStatus.Cancelada } },
            };

            if (validTransitions.ContainsKey(current) && !validTransitions[current].Contains(next))
                throw new DomainException($"Transição de status de '{current}' para '{next}' não é permitida.");
        }

        private ServiceOrderResponse MapToResponse(ServiceOrder entity)
        {
            return new ServiceOrderResponse
            {
                Id = entity.Id,
                TenantId = entity.TenantId,
                ClientId = entity.ClientId,
                ClientName = entity.Client?.Name ?? "",
                OrderDate = entity.OrderDate,
                TotalValue = entity.TotalValue,
                Status = entity.Status.ToString(),
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                ConcludedAt = entity.ConcludedAt,
                CreatedBy = entity.CreatedBy,
                Items = entity.ServiceOrderItems?.Select(i => new ServiceOrderItemResponse
                {
                    Id = i.Id,
                    ServiceProvidedId = i.ServiceProvidedId,
                    ServiceName = i.ServiceProvided?.Name ?? "",
                    ServiceDescription = i.ServiceProvided?.Description ?? "",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice,
                    LocationCode = i.ServiceProvided?.LocationCode ?? ""
                }).ToList() ?? new List<ServiceOrderItemResponse>(),
                Invoices = entity.ServiceInvoices?.Select(i => new ServiceInvoiceBriefResponse
                {
                    Id = i.Id,
                    NumeroDPS = i.NumeroDPS,
                    Status = i.Status.ToString(),
                    TotalValue = i.TotalValue,
                    EmittedAt = i.EmittedAt
                }).ToList() ?? new List<ServiceInvoiceBriefResponse>()
            };
        }
    }

    public interface IServiceOrderService : IBaseService<ServiceOrder>
    {
        Task<PagedResult<ServiceOrderResponse>> GetAllPaged(Filters filter);
        Task<ServiceOrderResponse> GetByIdAsync(int id);
        Task<ServiceOrderResponse> CreateAsync(ServiceOrderCreateRequest request, Guid userId);
        Task<ServiceOrderResponse> UpdateAsync(int id, ServiceOrderUpdateRequest request, Guid userId);
        Task<ServiceOrderResponse> ChangeStatusAsync(int id, ServiceOrderStatus newStatus, Guid userId);
        Task DeleteAsync(int id);
        Task<List<AvailableServiceResponse>> GetAvailableServices(int orderId);
    }
}
