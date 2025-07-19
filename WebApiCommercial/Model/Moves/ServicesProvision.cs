using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Moves
{
  public class ServicesProvision:BaseEntity
  {
    public DateTime Date { get; set; }
    public int? IdBudget { get; set; }
    public Budget Budget { get; set; }
    public string Description { get; set; }
    public int IdClient { get; set; }
    [NotMapped]
    public string NameClient { get; set; }

    [NotMapped]
    public string DescriptionBudget { get; set; }
    public Client Client { get; set; }
    public int IdCompany { get; set; }
    public Company Company { get; set; }
    public ICollection<ServicesProvisionItems> ServicesProvisionItems { get; set; }

  }
}
