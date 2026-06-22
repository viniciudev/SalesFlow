using Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    public class ServiceOrderCreateRequest
    {
        public int TenantId { get; set; }

        [Required(ErrorMessage = "Cliente é obrigatório")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Data da ordem é obrigatória")]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "Pelo menos um serviço é obrigatório")]
        [MinLength(1, ErrorMessage = "Pelo menos um serviço é obrigatório")]
        public List<ServiceOrderItemRequest> Items { get; set; } = new();
    }

    public class ServiceOrderUpdateRequest
    {
        public int ClientId { get; set; }
        public DateTime OrderDate { get; set; }
        public List<ServiceOrderItemRequest> Items { get; set; } = new();
    }

    public class ServiceOrderItemRequest
    {
        [Required]
        public int ServiceProvidedId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor unitário deve ser maior que zero")]
        public decimal UnitPrice { get; set; }
    }

    public class ServiceOrderResponse
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalValue { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ConcludedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public List<ServiceOrderItemResponse> Items { get; set; } = new();
        public List<ServiceInvoiceBriefResponse> Invoices { get; set; } = new();
    }

    public class ServiceOrderItemResponse
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

    public class ServiceInvoiceBriefResponse
    {
        public int Id { get; set; }
        public int NumeroDPS { get; set; }
        public string Status { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime? EmittedAt { get; set; }
    }

    public class ServiceOrderStatusRequest
    {
        [Required]
        public ServiceOrderStatus Status { get; set; }
    }
}
