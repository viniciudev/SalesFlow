using System.Collections.Generic;

namespace Model.Registrations
{
  public class DescriptionFiles : BaseEntity
  {
    public string NameProduct { get; set; }
    public string descriptionProduct { get; set; }
    public string valueProduct { get; set; }
    public int groupItems { get; set; }
    public int idCompany { get; set; }
    public Company Company { get; set; }
    public ICollection<File> Files { get; set; }
  }
}
