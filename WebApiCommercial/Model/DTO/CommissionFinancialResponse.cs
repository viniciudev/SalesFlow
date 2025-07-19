using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
  public  class CommissionFinancialResponse
  {
    public int IdFinancial { get; set; }
    public DateTime ReleaseDate { get; set; }
    public DateTime DateSale { get; set; }
    public DateTime DueDate { get; set; }
    public string Item { get; set; }
    public decimal Percentage { get; set; }
    public decimal Value { get; set; }
    public string Origin { get; set; }

  }
}
