using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class BankAccount:BaseEntity
    {
        public Company Company { get; set; }
        public int IdCompany { get; set; }
    }
}
