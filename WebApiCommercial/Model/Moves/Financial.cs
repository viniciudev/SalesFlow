using Microsoft.EntityFrameworkCore;
using Model;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Moves
{
    public class Financial : BaseEntity
    {
        public int IdCompany { get; set; }
        public Company Company { get; set; }
        public DateTime CreationDate { get; set; }
        public string Description { get; set; }
        public FinancialType FinancialType { get; set; }
        public int? IdCostCenter { get; set; }
        public CostCenter CostCenter { get; set; }
        public FinancialStatus FinancialStatus { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Value { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public int? PaymentMethodId { get; set; }
        public int? IdSalesman { get; set; }
        public Salesman Salesman { get; set; }
        public int? IdProduct { get; set; }
        public Product Product { get; set; }
        public int? IdService { get; set; }
        public ServiceProvided ServiceProvided { get; set; }
        public int? IdSale { get; set; }
        public Sale Sale { get; set; }
        public int? IdSaleItems { get; set; }
        public SaleItems SaleItems { get; set; }
        public decimal Percentage { get; set; }
        public bool commission { get; set; }
        public OriginFinancial Origin { get; set; }
        public int? BoxId { get; set; }
        public Box Box { get; set; }
        public int? IdClient { get; set; }
        public Client Client { get; set; }
        [NotMapped]
        public string ?PaymentType { get; set; }
        [NotMapped]
        public string PaymentMethodName { get; set; }
        [NotMapped]
        public string ClientName { get; set; }
    }


}


public enum FinancialType
{
    recipe = 0,
    expense = 1
}
public enum FinancialStatus
{
    pending = 1,
    paid = 2,
    renegotiated=3

}
//public enum PaymentType
//{
//    pix = 1,
//    card = 2,
//    cash = 3,

//}
public enum OriginFinancial
{
    commission = 0,
    financial = 1,
    renegotiation=2
}
