using Model.Closure;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
  public  class ClosuresRequest
  {
    public string LongInit { get; set; }
    public int LatInit { get; set; }
    public DateTime DateInit { get; set; }
    public Type Type { get; set; }
    public string LongFinal { get; set; }
    public int LatFinal { get; set; }
    public DateTime DateFinal { get; set; }
    //public ICollection<ClosuresDetail> ClosuresDetails { get; set; }
    public int IdSalesman { get; set; }
  }
}
