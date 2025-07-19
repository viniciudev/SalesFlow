using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
  public class SaleInfoResponse
  {
    public int SalesAmount { get; set; }
    public ICollection<int> AmountMonth { get; set; }
  }
}
