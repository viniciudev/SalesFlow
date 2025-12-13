using Model.Moves;
using System.Collections.Generic;

namespace Model.Registrations
{
    public class CostCenter : BaseEntity
    {
        [Uppercase]
        public string Name { get; set; }
        public int IdCompany { get; set; }
        public Company Company { get; set; }
        public ICollection<Commission> Commissions { get; set; }
        public ICollection<Financial> Financials { get; set; }
        public ICollection<SharedCommission> SharedCommissions { get; set; }
    }
}
