using Model.Moves;
using System.Collections.Generic;

namespace Model.Registrations
{
    public class ServiceProvided : BaseEntity
    {
        public int IdCompany { get; set; }
        public Company Company { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public string Deadline { get; set; }
        public string Capacity { get; set; }
        public string Experience { get; set; }
        public ICollection<DetailsService> Details { get; set; }
        public ICollection<SaleItems> SaleItems { get; set; }
        public ICollection<Commission> Commissions { get; set; }
        public ICollection<Financial> Financials { get; set; }
    }
}
