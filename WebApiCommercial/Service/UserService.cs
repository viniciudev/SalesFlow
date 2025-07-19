using Microsoft.IdentityModel.Tokens;
using Model;
using Model.Moves;
using Model.Registrations;
using Repository;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
   public class UserService : BaseService<User>, IUserService
  {
    private ICompanyService companyService;
    private IPlanCompanyService planCompanyService;
    private ICostCenterService costCenterService;
    public UserService(IGenericRepository<User> repository,
      ICompanyService companyService,
      IPlanCompanyService planCompanyService,
      ICostCenterService costCenterService) : base(repository)
      
    {
      this.companyService = companyService;
      this.planCompanyService = planCompanyService;
      this.costCenterService = costCenterService;
    }
      public Task<User> GetUser(AuthenticateModel model)
      {
         return (repository as IUserRepository).GetUser(model);
      }

    public async Task<AuthenticateResponse> Authenticate(AuthenticateModel model)
    {
      var user = await (repository as IUserRepository).GetUser(model);
      //v@v.com--1
      // return null if user not found
      if (user == null) return new AuthenticateResponse(new User(), "", "Usuário não localizado!"); ;

      Cryptography cryptography = new Cryptography();
      Boolean ComparaSenha = cryptography.authentic(user, model.Password);

      if (!ComparaSenha)
        return new AuthenticateResponse(user, "", "Senha inválida!");

      // authentication successful so generate jwt token

      var token = TokenService.GenerateToken(user);

      return new AuthenticateResponse(user, token);
    }

    private string GenerateJwtToken(User user, byte[] key)
    {
      // generate token that is valid for 7 days
      var tokenHandler = new JwtSecurityTokenHandler();
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
      };
      var token = tokenHandler.CreateToken(tokenDescriptor);
      return tokenHandler.WriteToken(token);
    }
    public async Task<string> SaveUser(User user)
    {
      AuthenticateModel authenticateModel = new AuthenticateModel();
      authenticateModel.Email = user.Email;

      var userExist = await (repository as IUserRepository).GetUser(authenticateModel);
      if (userExist != null)
        return "Usuário já possui cadastro!";

      Cryptography cryptography = new Cryptography();
      var hash = cryptography.addsEncrypted(user.Password);

      Company company = new Company();
      company.CorporateName = user.Name;
      Guid g = Guid.NewGuid();
      company.Guid= g;
      await companyService.Create(company);

      PlanCompany planCompany = new PlanCompany();
      planCompany.Status = (int)Statusplan.enable;
      planCompany.ExpirationDate=DateTime.Now.AddDays(30);
      planCompany.DateRegister = DateTime.Now;
      planCompany.LastPayment= DateTime.Now;
      planCompany.IdCompany = company.Id;
      await planCompanyService.Create(planCompany);

      CostCenter costCenter = new CostCenter();
      costCenter.Name = "Padrão"; 
      costCenter.IdCompany = company.Id;
      await costCenterService.Create(costCenter);

      user.IdCompany = company.Id;
      user.Password = hash;
      await base.Create(user);
      return "Salvo com Sucesso!";
    }
  }

  public interface IUserService : IBaseService<User>
  {
    Task<string> SaveUser(User user);
    Task<AuthenticateResponse> Authenticate(AuthenticateModel model);
  }

}
