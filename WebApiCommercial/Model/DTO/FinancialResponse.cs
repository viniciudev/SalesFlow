using Model.DTO.Financial;
using Model.Registrations;
using System;
using System.Collections.Generic;

namespace Model.DTO
{
    public class FinancialResponse
    {
        public decimal Value { get; set; }
        public int Id { get; set; }
        public FinancialStatus FinancialStatus { get; set; }
        public FinancialType FinancialType { get; set; }
        public OriginFinancial Origin { get; set; }
        public int? IdSale { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Description { get; set; }
        public List<string>? PaymentMethodName { get; set; }
        public List<int>? PaymentMethodId { get; set; }
        public int IdCompany { get; set; }
        public string ?ClientName { get; set; }
        public int? ClientId { get; set; }
        public  ICollection<FinancialResourcesResponse> FinancialResourcesResponseList { get; set; }
        public int? BankAccountId { get; set; }
		public List<PaymentsDto> PaymentMethods { get; set; }=new List<PaymentsDto>();
		public string SettlementDate { get; set; }
		public decimal InterestValue { get; set; }
		public decimal SettledValue { get; set; }
		public decimal FineValue { get; set; }
	}
    public class FinancialResourcesResponse {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
    }
}
