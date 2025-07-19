using System.ComponentModel.DataAnnotations;

namespace Model.Registrations
{
  public class Provider : BaseEntity
  {
    [Key]
    public int id { get; set; }
    public string nome { get; set; }
    public string logradouro { get; set; }
    public int numero { get; set; }
    public string bairro { get; set; }
    public int idcnae { get; set; }
    public string cidade { get; set; }

    public string nomecnae { get; set; }
  }
}
