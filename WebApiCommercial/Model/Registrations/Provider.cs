using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Model.Moves;

namespace Model.Registrations
{
  public class Provider : BaseEntity
  {
    [Key]
    public int id { get; set; }
    public string nome { get; set; }
    public string razaoSocial { get; set; }
    public string nomeFantasia { get; set; }
    public string cnpj { get; set; }
    public string inscricaoEstadual { get; set; }
    public string telefone { get; set; }
    public string email { get; set; }
    public string logradouro { get; set; }
    public int numero { get; set; }
    public string bairro { get; set; }
    public string cidade { get; set; }
    public string uf { get; set; }
    public string cep { get; set; }
    public string complemento { get; set; }
    public int idcnae { get; set; }
    public string nomecnae { get; set; }
    public int IdCompany { get; set; }
    public Company Company { get; set; }
    public ICollection<Purchase> Purchases { get; set; }
  }
}
