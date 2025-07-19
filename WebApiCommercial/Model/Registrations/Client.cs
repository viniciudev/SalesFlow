using Model.Moves;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model
{
  public class Client : BaseEntity
  {
    public int IdCompany { get; set; }
    public Company Company { get; set; }
    public string Document { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string CellPhone { get; set; }
    public string ZipCode { get; set; }
    public string Address { get; set; }
    public string Bairro { get; set; }
    public string Complement { get; set; }
    public string NameState { get; set; }
    public string NameCity { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime CreatDate { get; set; }
    public statusType Status { get; set; }
    public ICollection<ServicesProvision> ServiceProvisions { get; set; }
    public ICollection<Sale> Sale { get; set; }
    public enum statusType
    {
      Ativo,
      Inativo,
    }
  }

}
