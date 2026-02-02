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
        public PaymentType PaymentType { get; set; }
        public int IdCompany { get; set; }
        public string ?ClientName { get; set; }
        public int? ClientId { get; set; }
        public  ICollection<FinancialResourcesResponse> FinancialResourcesResponseList { get; set; }
    }
    public class FinancialResourcesResponse {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
    }
}
