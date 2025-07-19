using System.Collections.Generic;

namespace Model.DTO
{
  public class SalesCommissionsInfo
  {
    public int SalesAmountWeek { get; set; }
    public int CommissiontAmountWeek { get; set; }
    public ICollection<AmountOfCommissionsAndSales> AmountOfCommissionsAndSales { get; set; }
  }

  public class AmountOfCommissionsAndSales
  {
    public string Day { get; set; }
    public int Commissions { get; set; }
    public int Sales { get; set; }
  }
}
