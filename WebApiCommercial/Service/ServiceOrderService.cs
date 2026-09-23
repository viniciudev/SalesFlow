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
        private readonly IFinancialService _financialService;
        private readonly IServiceInvoiceService _serviceInvoiceService;

        public ServiceOrderService(
            IGenericRepository<ServiceOrder> repository,
            IServiceOrderRepository orderRepo,
            IServiceInvoiceRepository invoiceRepo,
            IFinancialService financialService,
            IServiceInvoiceService serviceInvoiceService) : base(repository)
        {
            _orderRepo = orderRepo;
            _invoiceRepo = invoiceRepo;
            _financialService = financialService;
            _serviceInvoiceService = serviceInvoiceService;
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

            var response = MapToResponse(entity);
            response.Financials = await LoadFinancialsAsync(id);
            return response;
        }

        public async Task<ServiceOrderResponse> CreateAsync(ServiceOrderCreateRequest request, Guid userId)
        {
            ValidateCreate(request);

            using (var transaction = await repository.CreateTransactionAsync())
            {
                try
                {
                    var entity = new ServiceOrder
                    {
                        TenantId = request.TenantId,
                        ClientId = request.ClientId,
                        OrderDate = request.OrderDate,
                        Notes = request.Notes,
                        Competence = request.Competence,
                        Status = ServiceOrderStatus.Aberta,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = userId,
                        UpdatedBy = userId,
                        ServiceOrderItems = new List<ServiceOrderItem>()
                    };

                    foreach (var item in request.Items)
                    {
                        entity.ServiceOrderItems.Add(BuildItem(item));
                    }

                    ServiceTotals.Apply(entity);

                    await repository.CreateAsync(entity);

                    // Ao contrário de vendas, o financeiro aqui é opcional: sem formas de
                    // pagamento informadas o gerador não cria nada e a OS segue normalmente.
                    await _financialService.GenerateFinancialCentral(request.FormPaymentSales, entity.TenantId,
                        idServiceOrder: entity.Id, idClient: entity.ClientId, descricaoOrigem: "Ordem de Serviço");

                    transaction.Commit();

                    var response = MapToResponse(entity);
                    response.Financials = await LoadFinancialsAsync(entity.Id);
                    return response;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<ServiceOrderResponse> UpdateAsync(int id, ServiceOrderUpdateRequest request, Guid userId)
        {
            var entity = await _orderRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("Ordem de serviço não encontrada.");

            if (entity.Status == ServiceOrderStatus.Concluida || entity.Status == ServiceOrderStatus.Cancelada)
                throw new DomainException("Não é possível editar uma ordem de serviço concluída ou cancelada.");

            using (var transaction = await repository.CreateTransactionAsync())
            {
                try
                {
                    entity.ClientId = request.ClientId;
                    entity.OrderDate = request.OrderDate;
                    entity.Notes = request.Notes;
                    entity.Competence = request.Competence;
                    entity.UpdatedAt = DateTime.UtcNow;
                    entity.UpdatedBy = userId;

                    var existingItems = entity.ServiceOrderItems.ToList();
                    foreach (var existing in existingItems)
                    {
                        var updatedItem = request.Items.FirstOrDefault(x => x.ServiceProvidedId == existing.ServiceProvidedId);
                        if (updatedItem != null)
                        {
                            // Copiar TODOS os campos fiscais, não só quantidade e valor: se o
                            // desconto/alíquota ficasse de fora, editar a OS apagaria a retenção
                            // que o usuário já tinha configurado na linha.
                            CopyFields(updatedItem, existing);
                        }
                    }

                    var existingServiceIds = existingItems.Select(x => x.ServiceProvidedId).ToList();
                    foreach (var item in request.Items.Where(x => !existingServiceIds.Contains(x.ServiceProvidedId)))
                    {
                        entity.ServiceOrderItems.Add(BuildItem(item));
                    }

                    var requestServiceIds = request.Items.Select(x => x.ServiceProvidedId).ToList();
                    var itemsToRemove = existingItems.Where(x => !requestServiceIds.Contains(x.ServiceProvidedId)).ToList();
                    foreach (var item in itemsToRemove)
                    {
                        entity.ServiceOrderItems.Remove(item);
                    }

                    ServiceTotals.Apply(entity);

                    await base.Alter(entity);

                    // Depois do Alter, e não antes: a OS já está com os itens finais
                    // (atualizados, adicionados e removidos) e é esse estado que a nota
                    // precisa espelhar.
                    await SyncPendingInvoicesAsync(entity);

                    await _financialService.ReplaceServiceOrderFinancials(id, entity.TenantId, entity.ClientId,
                        request.FormPaymentSales);

                    transaction.Commit();

                    var response = MapToResponse(entity);
                    response.Financials = await LoadFinancialsAsync(id);
                    return response;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Espelha nas NFS-e ainda pendentes desta OS os valores que acabaram de mudar na
        /// OS. Sem isso a nota — que é o documento que de fato vai ao SEFIN — continuaria
        /// com quantidade/valor/alíquota antigos e a emissão sairia errada.
        ///
        /// Três escolhas deliberadas:
        /// 1. Só as notas <see cref="ServiceInvoiceStatus.Pendente"/> são tocadas: nota
        ///    emitida é documento fiscal, e o próprio <c>ServiceInvoiceService.UpdateAsync</c>
        ///    recusa a edição ("Apenas NFSe pendente pode ser editada").
        /// 2. A seleção de serviços de cada nota é preservada. Uma OS pode ter várias notas
        ///    com conjuntos DISJUNTOS de serviços (a criação impede repetir um serviço já
        ///    faturado), então mandar a lista inteira da OS para cada nota duplicaria o
        ///    faturamento. Só os itens que a nota já fatura são atualizados.
        /// 3. Competência, ambiente e município continuam sendo os da NOTA: são dados
        ///    fiscais próprios do documento, editados na própria nota, e não derivados
        ///    da OS.
        /// </summary>
        private async Task SyncPendingInvoicesAsync(ServiceOrder order)
        {
            var invoices = await _invoiceRepo.GetInvoicesByOrderId(order.Id);
            if (invoices.Count == 0)
                return;

            foreach (var invoice in invoices.Where(x => x.Status == ServiceInvoiceStatus.Pendente))
            {
                var items = order.ServiceOrderItems
                    .Where(orderItem => invoice.ServiceInvoiceItems
                        .Any(invoiceItem => invoiceItem.ServiceProvidedId == orderItem.ServiceProvidedId))
                    .Select(orderItem => new ServiceInvoiceItemRequest
                    {
                        ServiceProvidedId = orderItem.ServiceProvidedId,
                        Quantity = orderItem.Quantity,
                        UnitPrice = orderItem.UnitPrice,
                        Discount = orderItem.Discount,
                        Description = orderItem.Description,
                        IssqnRate = orderItem.IssqnRate,
                        IssqnRetido = orderItem.IssqnRetido,
                        PisRate = orderItem.PisRate,
                        CofinsRate = orderItem.CofinsRate,
                        IrRate = orderItem.IrRate,
                        CsllRate = orderItem.CsllRate,
                        InssRate = orderItem.InssRate
                    })
                    .ToList();

                // Nenhum serviço da nota sobreviveu à edição da OS. Zerar a nota aqui a
                // deixaria inemissível por um efeito colateral de editar a OS; é melhor
                // deixá-la como está e o usuário decidir o que fazer com ela na tela da NFSe.
                if (items.Count == 0)
                    continue;

                await _serviceInvoiceService.UpdateAsync(invoice.Id, new ServiceInvoiceUpdateRequest
                {
                    DataCompetencia = invoice.DataCompetencia,
                    TipoAmbiente = invoice.TipoAmbiente,
                    CodMunIBGE = invoice.CodMunIBGE,
                    Items = items
                });
            }
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

            using (var transaction = await repository.CreateTransactionAsync())
            {
                try
                {
                    await base.Alter(entity);

                    // Cancelar por aqui também precisa anular os recebíveis: esta é a única
                    // transição que alcança Cancelada a partir de Concluida, então há parcelas
                    // já pagas que ficariam pendentes no financeiro.
                    if (newStatus == ServiceOrderStatus.Cancelada)
                        await _financialService.CancelServiceOrderFinancials(id);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return MapToResponse(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _orderRepo.GetByIdWithDetails(id);
            if (entity == null)
                throw new DomainException("Ordem de serviço não encontrada.");

            if (entity.Status == ServiceOrderStatus.Concluida)
                throw new DomainException("Não é possível cancelar uma ordem de serviço concluída.");

            using (var transaction = await repository.CreateTransactionAsync())
            {
                try
                {
                    entity.Status = ServiceOrderStatus.Cancelada;
                    entity.UpdatedAt = DateTime.UtcNow;
                    await base.Alter(entity);

                    // A OS e a anulação do financeiro sobem juntas ou não sobem: sem isto uma
                    // falha no financeiro deixaria a OS cancelada com recebíveis em aberto.
                    await _financialService.CancelServiceOrderFinancials(id);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
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
                NationalTaxCode = i.ServiceProvided?.NationalTaxCode ?? "",
                MunicipalTaxCode = i.ServiceProvided?.MunicipalTaxCode,
                NbsCode = i.ServiceProvided?.NbsCode,
                SpecialType = i.ServiceProvided?.SpecialType
            }).ToList();

            return items;
        }

        private static ServiceOrderItem BuildItem(ServiceOrderItemRequest item)
        {
            var entity = new ServiceOrderItem { CreatedAt = DateTime.UtcNow };
            CopyFields(item, entity);
            return entity;
        }

        /// <summary>Copia os campos do request para o item, incluindo os fiscais.</summary>
        private static void CopyFields(ServiceOrderItemRequest item, ServiceOrderItem entity)
        {
            entity.ServiceProvidedId = item.ServiceProvidedId;
            entity.Quantity = item.Quantity;
            entity.UnitPrice = item.UnitPrice;
            entity.Discount = item.Discount;
            entity.Description = item.Description;
            entity.IssqnRate = item.IssqnRate;
            entity.IssqnRetido = item.IssqnRetido;
            entity.PisRate = item.PisRate;
            entity.CofinsRate = item.CofinsRate;
            entity.IrRate = item.IrRate;
            entity.CsllRate = item.CsllRate;
            entity.InssRate = item.InssRate;

            // TotalPrice é sempre derivado, nunca recebido: assim o banco não guarda
            // um total que diverge da quantidade/valor/desconto gravados na mesma linha.
            entity.TotalPrice = ServiceTotals.ForItem(entity).Base;
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

                // Um desconto maior que o bruto deixaria a base do ISS negativa, e a DPS
                // seria rejeitada pelo SEFIN. Barrar aqui dá uma mensagem melhor.
                foreach (var item in request.Items)
                {
                    if (item.Discount > item.Quantity * item.UnitPrice)
                        errors.Add($"Desconto do serviço {item.ServiceProvidedId} não pode superar o valor bruto do item.");
                }
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

        /// <summary>
        /// Parcelas financeiras da OS no formato que a tela de edição espera. Parcelas
        /// canceladas ficam de fora: elas existem só como histórico do que já foi recebido.
        /// </summary>
        private async Task<List<ServiceOrderFinancialResponse>> LoadFinancialsAsync(int orderId)
        {
            var financials = await _financialService.GetByIdServiceOrderAsync(orderId);

            return financials
                .Where(f => f.FinancialStatus != FinancialStatus.Canceled)
                .Select(f =>
                {
                    var paymentMethod = f.FinancialPaymentMethods?.FirstOrDefault();
                    return new ServiceOrderFinancialResponse
                    {
                        Id = f.Id,
                        PaymentMethodId = paymentMethod?.PaymentMethodId ?? 0,
                        PaymentMethodName = paymentMethod?.PaymentMethod?.Name ?? "",
                        Value = f.Value,
                        DueDate = f.DueDate,
                        Status = f.FinancialStatus
                    };
                })
                .ToList();
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
                Notes = entity.Notes,
                Competence = entity.Competence,
                TotalValue = entity.TotalValue,
                DiscountValue = entity.DiscountValue,
                IssqnValue = entity.IssqnValue,
                IssqnRetidoValue = entity.IssqnRetidoValue,
                RetentionValue = entity.RetentionValue,
                NetValue = entity.NetValue,
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
                    Discount = i.Discount,
                    Description = i.Description,
                    TotalPrice = i.TotalPrice,
                    IssqnRate = i.IssqnRate,
                    IssqnRetido = i.IssqnRetido,
                    IssqnValue = ServiceTotals.ForItem(i).Issqn,
                    PisRate = i.PisRate,
                    CofinsRate = i.CofinsRate,
                    IrRate = i.IrRate,
                    CsllRate = i.CsllRate,
                    InssRate = i.InssRate,
                    LocationCode = i.ServiceProvided?.LocationCode ?? "",
                    NationalTaxCode = i.ServiceProvided?.NationalTaxCode ?? ""
                }).ToList() ?? new List<ServiceOrderItemResponse>(),
                Invoices = entity.ServiceInvoices?.Select(i => new ServiceInvoiceBriefResponse
                {
                    Id = i.Id,
                    NumeroDPS = i.NumeroDPS,
                    Status = i.Status.ToString(),
                    TotalValue = i.TotalValue,
                    EmittedAt = i.EmittedAt,
                    ChaveAcesso = i.ChaveAcesso,
                    Sent = i.Sent,
                    ErrorMessage = i.ErrorMessage
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
