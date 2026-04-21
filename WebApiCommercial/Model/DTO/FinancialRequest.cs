using Model.DTO.Financial;
using Model.Registrations;
using System;
using System.Collections.Generic;

namespace Model.DTO
{
    public class FinancialRequest
    {
        public int Id { get; set; }
        public decimal Value { get; set; }
        public FinancialStatus FinancialStatus { get; set; }
        public FinancialType FinancialType { get; set; }
        public OriginFinancial Origin { get; set; }
        public int? IdSale { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Description { get; set; }
        public string PaymentType { get; set; }
        public int IdCompany { get; set; }
        public int? ClientId { get; set; }
		public virtual ICollection<PaymentsDto> PaymentMethods { get; set; } = new List<PaymentsDto>();

		public int? BankAccountId { get; set; }
		public string SettlementDate { get; set; }
		public decimal InterestValue { get; set; }
		public decimal FineValue { get; set; }
		public decimal SettledValue { get; set; }
	}
}
