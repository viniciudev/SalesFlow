using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
  public class Filters
  {
    public string textOption { get; set; }
    public FilterType selectOption { get; set; }
    public string cellPhoneOption { get; set; }
    public int pageNumber { get; set; }
    public int pageSize { get; set; }
    public int codGroup { get; set; }
    public int IdSale { get; set; }
    public int IdSalesman { get; set; }
    public Filters()
    {
      this.pageNumber = 1;
      this.pageSize = 10;
    }
    public int idCompany { get; set; }
    public int idBudget { get; set; }
    public int idServiceProvision { get; set; }
    public int idClient { get; set; }
    public DateTime SaleDate { get; set; }
    public DateTime SaleDateFinal { get; set; }
		public DateTime CheckinDate { get; set; }
		public DateTime CheckinDateFinal { get; set; }
		public int IdSeller { get; set; }
  }

  public enum FilterType
  {
    Name,
    Cpf,
  }

}
