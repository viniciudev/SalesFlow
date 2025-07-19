using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Model.Registrations
{
  public class Commission : BaseEntity
  {
    public int? IdService { get; set; }
    public ServiceProvided ServiceProvided { get; set; }
    public int? IdProduct { get; set; }
    public Product Product { get; set; }
    public int IdSalesman { get; set; }
    public Salesman Salesman { get; set; }
    public decimal Percentage { get; set; }
    public int Status { get; set; }
    public int CommissionDay { get; set; }
    public TypeDay TypeDay { get; set; }
    public int IdCostCenter{ get; set; }
    public CostCenter CostCenter { get; set; }
  }
  public enum TypeDay
  {
    run = 0,
    fixedDay = 1
  }
}
