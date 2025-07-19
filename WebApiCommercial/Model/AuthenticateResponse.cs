using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
  public class AuthenticateResponse
  {
    public int IdUser { get; set; }
    public int IdCompany { get; set; }
    public string Name { get; set; }
    public string CellPhone { get; set; }
    public string UserName { get; set; }
    public string Token { get; set; }
    public string Message { get; set; }
		public Guid Guid { get; set; }

		public AuthenticateResponse(User user, string token, string message = null)
    {
      IdUser = user.Id;
      Name = user.Name;
      CellPhone = user.CellPhone;
      UserName = user.Email;
      Token = token;
      IdCompany = user.IdCompany;
      Message = message;
      Guid = user.Company!=null? user.Company.Guid:new Guid();
    }
  }
}
