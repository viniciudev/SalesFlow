using Model.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Registrations
{
  public class ServiceProvided:BaseEntity
  {
    public int IdCompany { get; set; }
    public Company Company { get; set; }
    public string Name { get; set; }
    public decimal Value { get; set; }
    public ICollection<SaleItems> SaleItems { get; set; }
    public ICollection<Commission> Commissions { get; set; }
    public ICollection<Financial> Financials { get; set; }
  }
}
