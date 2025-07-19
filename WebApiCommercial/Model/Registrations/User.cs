using Model.Registrations;
using System;

namespace Model
{
  public class User : BaseEntity
  {
    public string Name { get; set; }
    public string CellPhone { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public DateTime? BirthDate { get; set; }
    public string Role { get; set; }
    public Company Company { get; set; }
    public int IdCompany { get; set; }
  }
}
