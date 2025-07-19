using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
  public class CommissionResponse
  {
    public int Id { get; set; }
    public int IdService { get; set; }
    public string DescriptionService { get; set; }
    public int IdProduct { get; set; }
    public string DescriptionProduct { get; set; }
    public decimal Percentage { get; set; }
    public int Status { get; set; }
    public int IdCostCenter { get; set; }
    public int CommissionDay { get; set; }
    public int TypeDay { get; set; }
    public Product Product { get; set; }
  }
}
