using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Moves
{
  public class PlanCompany:BaseEntity
  {
    public int IdCompany { get; set; }
    public Company Company { get; set; }
    public DateTime DateRegister { get; set; }
    public DateTime ExpirationDate  { get; set; }
    public DateTime LastPayment { get; set; }
    public int Status { get; set; }
  }
  public enum Statusplan
  {
    enable=0,
    disable=1,
  }
}
