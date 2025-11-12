using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Moves
{
    public class Stock:BaseEntity
    {
        public int IdProduct { get; set; }
        public Product Product { get; set; }
        public int IdCompany { get; set; }
        public Company Company { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
        public DateTime Date { get; set; }
        public StockType Type { get; set; }
        [NotMapped]
        public string ProductName { get; set; }
        [NotMapped]
        public decimal Balance { get; set; }
    }
    public enum StockType
    {
        entry= 1,
        exit=2
    }
}
