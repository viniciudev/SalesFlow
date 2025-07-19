using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Moves
{
  public class SharedCommission : BaseEntity
  {
    public int IdSaleItems { get; set; }
    public SaleItems SaleItems { get; set; }
    public int IdSalesman { get; set; }
    public Salesman Salesman { get; set; }
    public decimal Percentage { get; set; }
    public int CommissionDay { get; set; }
    public TypeDay TypeDay { get; set; }
    public int IdCostCenter { get; set; }
    public bool EnableSharedCommission { get; set; }
    public int RecurringAmount { get; set; }
    public CostCenter CostCenter { get; set; }
    public string NameSeller { get; set; }
  }

}
