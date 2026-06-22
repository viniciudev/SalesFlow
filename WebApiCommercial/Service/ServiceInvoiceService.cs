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
    public class ServiceInvoiceService : BaseService<ServiceInvoice>, IServiceInvoiceService
    {
        private readonly IServiceInvoiceRepository _invoiceRepo;
        private readonly IServiceOrderRepository _orderRepo;

        public ServiceInvoiceService(
            IGenericRepository<ServiceInvoice> repository,
            IServiceInvoiceRepository invoiceRepo,
            IServiceOrderRepository orderRepo) : base(repository)
        {
            _invoiceRepo = invoiceRepo;
            _orderRepo = orderRepo;
        }

        public async Task<PagedResult<ServiceInvoiceResponse>> GetAllPaged(Filters filter)
        {
            var paged = await _invoiceRepo.GetAllPaged(filter);
            var responses = new PagedResult<ServiceInvoiceResponse>
            {
                CurrentPage = paged.CurrentPage,
                PageCount = paged.PageCount,
                PageSize = paged.PageSize,
                RowCount = paged.RowCount,
                Results = paged.Results.Select(MapToResponse).ToList()
            };
            return responses;
        }

        public async Task<ServiceInvoiceResponse> GetByIdAsync(int id)
        {
            var entity = await _invoiceRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("NFSe não encontrada.");
            return MapToResponse(entity);
        }

        public async Task<ServiceInvoiceResponse> CreateAsync(ServiceInvoiceCreateRequest request, Guid userId)
        {
            var order = await _orderRepo.GetByIdWithDetails(request.ServiceOrderId);
            if (order == null)
                throw new DomainException("Ordem de serviço não encontrada.");

            if (order.Status == ServiceOrderStatus.Cancelada)
                throw new DomainException("Não é possível criar NFSe para uma ordem cancelada.");

            // Get CodMunIBGE from first service in order
            var firstService = order.ServiceOrderItems.FirstOrDefault();
            var codMunIBGE = firstService?.ServiceProvided?.LocationCode ?? "";

            // Check for duplicate services
            var orderServiceIds = order.ServiceOrderItems.Select(x => x.ServiceProvidedId).ToList();
            foreach (var serviceId in orderServiceIds)
            {
                var alreadyEmitted = await _invoiceRepo.HasEmittedInvoicesForService(request.ServiceOrderId, serviceId);
                if (alreadyEmitted)
                    throw new DomainException($"Serviço ID {serviceId} já foi emitido em outra NFSe desta ordem.");
            }

            var entity = new ServiceInvoice
            {
                ServiceOrderId = request.ServiceOrderId,
                TenantId = request.TenantId,
                ClientId = order.ClientId,
                IdDPS = Guid.NewGuid().ToString("N").Substring(0, 20),
                TipoAmbiente = request.TipoAmbiente,
                CodMunIBGE = codMunIBGE,
                Status = ServiceInvoiceStatus.Pendente,
                NumeroDPS = 0,
                DataCompetencia = request.DataCompetencia,
                TotalValue = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                DhEmissao = null,
                ServiceInvoiceItems = new List<ServiceInvoiceItem>()
            };

            // Copy all services from order to invoice
            foreach (var orderItem in order.ServiceOrderItems)
            {
                entity.ServiceInvoiceItems.Add(new ServiceInvoiceItem
                {
                    ServiceProvidedId = orderItem.ServiceProvidedId,
                    Quantity = orderItem.Quantity,
                    UnitPrice = orderItem.UnitPrice,
                    TotalPrice = orderItem.TotalPrice,
                    CreatedAt = DateTime.UtcNow
                });
            }

            entity.TotalValue = entity.ServiceInvoiceItems.Sum(x => x.TotalPrice);

            await repository.CreateAsync(entity);

            return MapToResponse(entity);
        }

        public async Task<ServiceInvoiceResponse> UpdateAsync(int id, ServiceInvoiceUpdateRequest request)
        {
            var entity = await _invoiceRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("NFSe não encontrada.");

            if (entity.Status != ServiceInvoiceStatus.Pendente)
                throw new DomainException("Apenas NFSe pendente pode ser editada.");

            entity.DataCompetencia = request.DataCompetencia;
            entity.TipoAmbiente = request.TipoAmbiente;
            entity.CodMunIBGE = request.CodMunIBGE ?? entity.CodMunIBGE;
            entity.UpdatedAt = DateTime.UtcNow;

            if (request.Items != null && request.Items.Count > 0)
            {
                var newItems = request.Items.Select(i => new ServiceInvoiceItem
                {
                    ServiceInvoiceId = id,
                    ServiceProvidedId = i.ServiceProvidedId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.Quantity * i.UnitPrice,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _invoiceRepo.UpdateInvoiceItems(id, newItems);
                entity.TotalValue = newItems.Sum(x => x.TotalPrice);
            }

            await base.Alter(entity);

            return MapToResponse(entity);
        }

        public async Task<ServiceInvoiceResponse> EmitirAsync(int id, Guid userId)
        {
            var entity = await _invoiceRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("NFSe não encontrada.");

            if (entity.Status != ServiceInvoiceStatus.Pendente)
                throw new DomainException("Apenas NFSe pendente pode ser emitida.");

            if (!entity.ServiceInvoiceItems.Any())
                throw new DomainException("NFSe deve ter pelo menos um serviço.");

            // Generate DPS number
            var nextNumber = await _invoiceRepo.GetNextInvoiceNumber(entity.TenantId);
            entity.NumeroDPS = nextNumber;
            entity.DhEmissao = DateTime.UtcNow;
            entity.EmittedAt = DateTime.UtcNow;
            entity.Status = ServiceInvoiceStatus.Emitido;
            entity.UpdatedAt = DateTime.UtcNow;

            // Increment counter
            await _invoiceRepo.IncrementInvoiceNumber(entity.TenantId);

            await base.Alter(entity);

            // Check if all invoices of this order are emitted, if so, conclude the order
            var order = await _orderRepo.GetByIdWithDetails(entity.ServiceOrderId);
            if (order != null && order.Status != ServiceOrderStatus.Concluida)
            {
                var allEmitted = order.ServiceInvoices.All(x => x.Status == ServiceInvoiceStatus.Emitido);
                if (allEmitted)
                {
                    order.Status = ServiceOrderStatus.Concluida;
                    order.ConcludedAt = DateTime.UtcNow;
                    order.UpdatedAt = DateTime.UtcNow;
                    order.UpdatedBy = userId;
                    // Update the order through its own repository
                    await _orderRepo.UpdateAsync(order.Id, order);
                }
            }

            return MapToResponse(entity);
        }

        public async Task<ServiceInvoiceResponse> CancelarAsync(int id, string cancelReason, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(cancelReason) || cancelReason.Length < 15)
                throw new DomainException("Justificativa de cancelamento deve ter no mínimo 15 caracteres.");

            if (cancelReason.Length > 500)
                throw new DomainException("Justificativa de cancelamento deve ter no máximo 500 caracteres.");

            var entity = await _invoiceRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("NFSe não encontrada.");

            if (entity.Status == ServiceInvoiceStatus.Cancelado)
                throw new DomainException("NFSe já está cancelada.");

            entity.Status = ServiceInvoiceStatus.Cancelado;
            entity.CancelReason = cancelReason;
            entity.CanceledBy = userId;
            entity.UpdatedAt = DateTime.UtcNow;

            await base.Alter(entity);

            return MapToResponse(entity);
        }

        public async Task<ServiceInvoiceResponse> ResendAsync(int id)
        {
            var entity = await _invoiceRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("NFSe não encontrada.");

            if (entity.Status != ServiceInvoiceStatus.Emitido)
                throw new DomainException("Apenas NFSe emitida pode ser reenviada.");

            // Use current ServiceProvided data to update items
            foreach (var item in entity.ServiceInvoiceItems)
            {
                if (item.ServiceProvided != null)
                {
                    item.UnitPrice = item.ServiceProvided.Value;
                    item.TotalPrice = item.Quantity * item.ServiceProvided.Value;
                }
            }

            entity.TotalValue = entity.ServiceInvoiceItems.Sum(x => x.TotalPrice);
            entity.UpdatedAt = DateTime.UtcNow;

            await base.Alter(entity);

            return MapToResponse(entity);
        }

        public async Task<int> GetNextNumber(int tenantId)
        {
            return await _invoiceRepo.GetNextInvoiceNumber(tenantId);
        }

        private ServiceInvoiceResponse MapToResponse(ServiceInvoice entity)
        {
            return new ServiceInvoiceResponse
            {
                Id = entity.Id,
                ServiceOrderId = entity.ServiceOrderId,
                TenantId = entity.TenantId,
                ClientId = entity.ClientId,
                ClientName = entity.Client?.Name ?? "",
                ClientDocument = entity.Client?.Document ?? "",
                IdDPS = entity.IdDPS,
                TipoAmbiente = entity.TipoAmbiente.ToString(),
                DhEmissao = entity.DhEmissao,
                CodMunIBGE = entity.CodMunIBGE,
                Status = entity.Status.ToString(),
                NumeroDPS = entity.NumeroDPS,
                DataCompetencia = entity.DataCompetencia,
                TotalValue = entity.TotalValue,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                EmittedAt = entity.EmittedAt,
                CancelReason = entity.CancelReason,
                CanceledBy = entity.CanceledBy,
                CreatedBy = entity.CreatedBy,
                Items = entity.ServiceInvoiceItems?.Select(i => new ServiceInvoiceItemResponse
                {
                    Id = i.Id,
                    ServiceProvidedId = i.ServiceProvidedId,
                    ServiceName = i.ServiceProvided?.Name ?? "",
                    ServiceDescription = i.ServiceProvided?.Description ?? "",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice,
                    LocationCode = i.ServiceProvided?.LocationCode ?? ""
                }).ToList() ?? new List<ServiceInvoiceItemResponse>()
            };
        }
    }

    public interface IServiceInvoiceService : IBaseService<ServiceInvoice>
    {
        Task<PagedResult<ServiceInvoiceResponse>> GetAllPaged(Filters filter);
        Task<ServiceInvoiceResponse> GetByIdAsync(int id);
        Task<ServiceInvoiceResponse> CreateAsync(ServiceInvoiceCreateRequest request, Guid userId);
        Task<ServiceInvoiceResponse> UpdateAsync(int id, ServiceInvoiceUpdateRequest request);
        Task<ServiceInvoiceResponse> EmitirAsync(int id, Guid userId);
        Task<ServiceInvoiceResponse> CancelarAsync(int id, string cancelReason, Guid userId);
        Task<ServiceInvoiceResponse> ResendAsync(int id);
        Task<int> GetNextNumber(int tenantId);
    }
}
