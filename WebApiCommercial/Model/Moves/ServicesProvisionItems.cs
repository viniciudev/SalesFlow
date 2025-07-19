using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Moves
{
  public class ServicesProvisionItems:BaseEntity
  {
    public int IdServiceProvision { get; set; }
    public ServicesProvision ServiceProvision { get; set; }
    public TypeItemServices TypeItem { get; set; }
    public int IdItem { get; set; }
    public decimal Value { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    [NotMapped]
    public string NameItem { get; set; }
  }
  public enum TypeItemServices
  {
    Product,
    Service,
  }
}

