using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Registrations
{
    public class PaymentMethod:BaseEntity
    {
        public string Name { get; set; }
        public int IdCompany { get; set; }
        public Company Company { get; set; }
    }
}
