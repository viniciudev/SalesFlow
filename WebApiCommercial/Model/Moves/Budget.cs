using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Moves
{
  public class Budget:BaseEntity
  {
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public int IdCompany { get; set; }
    public int IdClient { get; set; }
    [NotMapped]
    public string NameClient { get; set; }
    public Company Company { get; set; }
    public ICollection<BudgetItems> BudgetItems { get; set; }
    public ICollection<ServicesProvision> ServiceProvisions { get; set; }
  }
}
