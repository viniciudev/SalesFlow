using Model.Registrations;
using System;

namespace Model
{
	public class Filters
	{
		public string? TextOption { get; set; }
		public FilterType SelectOption { get; set; }
		public string? cellPhoneOption { get; set; }
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
		public FinancialType? FinancialType { get; set; }
		public FinancialStatus? FinancialStatus { get; set; }
		public StatusNfe? StatusNfe { get; set; }
		public string? StartDate { get; set; }      // Formato: "yyyy-MM-dd"
		public string? EndDate { get; set; }        // Formato: "yyyy-MM-dd"
		public int? ClientId { get; set; }
		public int? PaymentMethodId { get; set; }
		public int? BankAccountId { get; set; }
	}

	public enum FilterType
	{
		Name,
		Cpf,
	}

}
