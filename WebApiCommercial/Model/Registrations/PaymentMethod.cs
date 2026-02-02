


using Model.Moves;
using System.Collections.Generic;

namespace Model.Registrations
{
    public class PaymentMethod : BaseEntity
    {
        public string Name { get; set; }
        public int IdCompany { get; set; }
        public Company Company { get; set; }
        public ICollection<Financial> Financials { get; set; } = new List<Financial>();
    }
}
