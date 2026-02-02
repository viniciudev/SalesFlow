using System;

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
        public int? PaymentMethodId { get; set; }
    }
}
