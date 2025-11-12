using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
   
    public class StockSummary
    {
        public int IdProduct { get; set; }
        public string ProductName { get; set; }
        public decimal TotalEntradas { get; set; }
        public decimal TotalSaidas { get; set; }
        public decimal SaldoAtual { get; set; }
    }
}
