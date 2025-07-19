using Model.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Registrations
{
  public class CostCenter:BaseEntity
  {
    public string Name { get; set; }
    public int IdCompany { get; set; }
    public Company Company { get; set; }
    public ICollection<Commission> Commissions { get; set; }
    public ICollection<Financial> Financials { get; set; }
    public ICollection<SharedCommission> SharedCommissions { get; set; }
  }
}
