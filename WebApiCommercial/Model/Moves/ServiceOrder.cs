using Model.Enums;
using Model.Registrations;
using System;
using System.Collections.Generic;

namespace Model.Moves
{
    public class ServiceOrder : BaseEntity
    {
        public int TenantId { get; set; }
        public Company Company { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalValue { get; set; }
        public ServiceOrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ConcludedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
        public ICollection<ServiceOrderItem> ServiceOrderItems { get; set; }
        public ICollection<ServiceInvoice> ServiceInvoices { get; set; }
    }
}
