using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Registrations
{
  public class Prospects:BaseEntity
  {
    public int IdCompany { get; set; }
    public Company Company { get; set; }
    public DateTime RegisterDate { get; set; }
    public string Name { get; set; }
    public string CellPhone { get; set; }
    public ICollection<PhasesProspects> PhasesProspects { get; set; }
  }
}
