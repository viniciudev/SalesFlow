using Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    public class ServiceInvoiceCreateRequest
    {
        [Required(ErrorMessage = "Ordem de serviço é obrigatória")]
        public int ServiceOrderId { get; set; }

        public int TenantId { get; set; }

        [Required(ErrorMessage = "Data de competência é obrigatória")]
        public DateTime DataCompetencia { get; set; }

        public AmbienteEnum TipoAmbiente { get; set; } = AmbienteEnum.Producao;
    }

    public class ServiceInvoiceUpdateRequest
    {
        public DateTime DataCompetencia { get; set; }
        public AmbienteEnum TipoAmbiente { get; set; }
        public string CodMunIBGE { get; set; }
        public List<ServiceInvoiceItemRequest> Items { get; set; } = new();
    }

    public class ServiceInvoiceItemRequest
    {
        [Required]
        public int ServiceProvidedId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }

    public class ServiceInvoiceResponse
    {
        public int Id { get; set; }
        public int ServiceOrderId { get; set; }
        public int TenantId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientDocument { get; set; }
        public string IdDPS { get; set; }
        public string TipoAmbiente { get; set; }
        public DateTime? DhEmissao { get; set; }
        public string CodMunIBGE { get; set; }
        public string Status { get; set; }
        public int NumeroDPS { get; set; }
        public DateTime DataCompetencia { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? EmittedAt { get; set; }
        public string CancelReason { get; set; }
        public Guid? CanceledBy { get; set; }
        public Guid CreatedBy { get; set; }
        public List<ServiceInvoiceItemResponse> Items { get; set; } = new();
    }

    public class ServiceInvoiceItemResponse
    {
        public int Id { get; set; }
        public int ServiceProvidedId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string LocationCode { get; set; }
    }

    public class ServiceInvoiceStatusRequest
    {
        [Required]
        public ServiceInvoiceStatus Status { get; set; }

        public string CancelReason { get; set; }
    }

    public class ServiceInvoiceCancelRequest
    {
        [Required(ErrorMessage = "Justificativa de cancelamento é obrigatória")]
        [MinLength(15, ErrorMessage = "Justificativa deve ter no mínimo 15 caracteres")]
        [MaxLength(500, ErrorMessage = "Justificativa deve ter no máximo 500 caracteres")]
        public string CancelReason { get; set; }
    }

    public class AvailableServiceResponse
    {
        public int ServiceProvidedId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
        public string LocationCode { get; set; }
        public string NationalTaxCode { get; set; }
    }
}
