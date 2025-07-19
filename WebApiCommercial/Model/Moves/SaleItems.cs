using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Moves
{
  public class SaleItems:BaseEntity
  {
    public decimal Value { get; set; }
    public int Amount { get; set; }
    public DateTime InclusionDate { get; set; }
    public int IdSale { get; set; }
    public Sale Sale { get; set; }
    public int? IdProduct { get; set; }
    public Product Product { get; set; }
    public int? IdService { get; set; }
    public int TypeItem { get; set; }
    public int RecurringAmount { get; set; }
    public bool EnableRecurrence { get; set; }
    public ServiceProvided ServiceProvided { get; set; }
    [NotMapped]
    public string NameItem { get; set; }
    [NotMapped]
    public int IdSeller { get; set; }
    public ICollection<Financial> Financials { get; set; }
    public ICollection<SharedCommission> SharedCommissions { get; set; }
  }
}
