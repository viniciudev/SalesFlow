using Model.Moves;
using System;
using System.Collections.Generic;

namespace Model.Registrations
{
  public class Company : BaseEntity
  {
    public string CorporateName { get; set; }
    public Guid Guid { get; set; }
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
  }
}
