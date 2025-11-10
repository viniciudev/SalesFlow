using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Moves
{
    public class Stock:BaseEntity
    {
        public int IdProduct { get; set; }
        public Product Product { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public int MyProperty { get; set; }
        public int Movement { get; set; }
    }
}
