using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Registrations
{
  public  class PhasesProspects:BaseEntity
  {
    public int IdProspects { get; set; }
    public Prospects Prospects { get; set; }
    public string Info { get; set; }

  }
}
