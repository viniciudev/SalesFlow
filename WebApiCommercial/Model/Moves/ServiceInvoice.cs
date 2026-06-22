using Model.Enums;
using Model.Registrations;
using System;
using System.Collections.Generic;

namespace Model.Moves
{
    public class ServiceInvoice : BaseEntity
    {
        public int ServiceOrderId { get; set; }
        public ServiceOrder ServiceOrder { get; set; }
        public int TenantId { get; set; }
        public Company Company { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public string IdDPS { get; set; }
        public AmbienteEnum TipoAmbiente { get; set; }
        public DateTime? DhEmissao { get; set; }
        public string CodMunIBGE { get; set; }
        public ServiceInvoiceStatus Status { get; set; }
        public int NumeroDPS { get; set; }
        public DateTime DataCompetencia { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? EmittedAt { get; set; }
        public string CancelReason { get; set; }
        public Guid? CanceledBy { get; set; }
        public Guid CreatedBy { get; set; }
        public ICollection<ServiceInvoiceItem> ServiceInvoiceItems { get; set; }
    }
}
