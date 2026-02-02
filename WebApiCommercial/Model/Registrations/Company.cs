using Model.Moves;
using System;
using System.Collections.Generic;

namespace Model.Registrations
{
    public class Company : BaseEntity
    {
        public string CorporateName { get; set; }

        public Guid Guid { get; set; }
        [Uppercase]
        public string? Name { get; set; }
        public string? Cnpj { get; set; }
        public string? ZipCode { get; set; }
        public string? Address { get; set; }
        public string? State { get; set; }
        public string? CommercialPhone { get; set; }
        public string? City { get; set; }
        public string? Cellphone { get; set; }
        public string? Ie { get; set; }
        public ICollection<DescriptionFiles> DescriptionFiles { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<Product> Products { get; set; }
        public ICollection<ServiceProvided> ServiceProvideds { get; set; }
        public ICollection<Client> Clients { get; set; }
        public ICollection<Budget> Budgets { get; set; }
        public ICollection<ServicesProvision> ServiceProvisions { get; set; }
        public ICollection<Salesman> Salesmen { get; set; }
        public ICollection<Sale> Sale { get; set; }
        public ICollection<CostCenter> CostCenters { get; set; }
        public ICollection<Financial> Financials { get; set; }
        public PlanCompany PlanCompany { get; set; }
        public ICollection<Prospects> Prospects { get; set; }
        public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
        public ICollection<Box> Boxes { get; set; } = new List<Box>();
        public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
    }
}
