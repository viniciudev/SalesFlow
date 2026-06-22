using Model.Registrations;
using System;

namespace Model.Moves
{
    public class ServiceInvoiceItem : BaseEntity
    {
        public int ServiceInvoiceId { get; set; }
        public ServiceInvoice ServiceInvoice { get; set; }
        public int ServiceProvidedId { get; set; }
        public ServiceProvided ServiceProvided { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
