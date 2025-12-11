using System;

namespace Model
{
    public class Filters
    {
        public string TextOption { get; set; }
        public FilterType SelectOption { get; set; }
        public string cellPhoneOption { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int CodGroup { get; set; }
        public int IdSale { get; set; }
        public int IdSalesman { get; set; }
      
        public int IdCompany { get; set; }
        public int IdBudget { get; set; }
        public int IdServiceProvision { get; set; }
        public int IdClient { get; set; }
        public DateTime SaleDate { get; set; }
        public DateTime SaleDateFinal { get; set; }
        public DateTime CheckinDate { get; set; }
        public DateTime CheckinDateFinal { get; set; }
        public int IdSeller { get; set; }
        public FinancialType ?FinancialType  { get; set; }
        public FinancialStatus ?FinancialStatus { get; set; }
    }

    public enum FilterType
    {
        Name,
        Cpf,
    }

}
