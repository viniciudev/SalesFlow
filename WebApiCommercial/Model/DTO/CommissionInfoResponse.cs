using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
  public class CommissionInfoResponse
  {
    public int CommissionAmount { get; set; }
    public ICollection<int> AmountMonth { get; set; }
  }
}
