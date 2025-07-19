using Model.Moves;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
  public class RequestSaveSale
  {
    public int IdCompany { get; set; }
    public Company Company { get; set; }
    public DateTime ReleaseDate { get; set; }
    public DateTime SaleDate { get; set; }
    public int IdClient { get; set; }
    public Client Client { get; set; }
    public int? IdSeller { get; set; }
    public Salesman Salesman { get; set; }
    public ICollection<SaleItems> SaleItems { get; set; }
  }
}
