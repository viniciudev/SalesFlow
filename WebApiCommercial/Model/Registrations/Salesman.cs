using Model.Moves;
using Model.Closure;
using System.Collections.Generic;

namespace Model.Registrations
{
  public class Salesman : BaseEntity
  {
    public string Name { get; set; }
    public string Document { get; set; }
    public string ZipCode { get; set; }
    public string Address { get; set; }
    public string Bairro { get; set; }
    public string NameState { get; set; }
    public string NameCity { get; set; }
    public string Telephone { get; set; }
    public int IdCompany { get; set; }
    public Company Company { get; set; }
    public ICollection<Sale> Sale { get; set; }
    public ICollection<Commission> Commissions { get; set; }
    public ICollection<Financial> Financials { get; set; }
    public ICollection<SharedCommission> SharedCommissions { get; set; }
    public ICollection<Closures> Closures { get; set; }
    //produtos
  }
}
