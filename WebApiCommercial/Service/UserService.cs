using Microsoft.IdentityModel.Tokens;
using Model;
using Model.DTO;
using Model.Moves;
using Model.Registrations;
using Repository;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Service
{
    public class UserService : BaseService<User>, IUserService
    {
        private ICompanyService companyService;
        private IPlanCompanyService planCompanyService;
        private ICostCenterService costCenterService;
        private IEmailService emailService;
        public UserService(IGenericRepository<User> repository,
          ICompanyService companyService,
          IPlanCompanyService planCompanyService,
          ICostCenterService costCenterService,
          IEmailService emailService) : base(repository)

        {
            this.companyService = companyService;
            this.planCompanyService = planCompanyService;
            this.costCenterService = costCenterService;
            this.emailService = emailService;
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
            if (user == null) return new AuthenticateResponse(new User(), "", "Usuário não localizado!");
            if (!user.VerifiedEmail) return new AuthenticateResponse(new User(), "", "Email não verificado!");
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
            company.Guid = g;
            await companyService.Create(company);

            PlanCompany planCompany = new PlanCompany();
            planCompany.Status = (int)Statusplan.enable;
            planCompany.ExpirationDate = DateTime.Now.AddDays(30);
            planCompany.DateRegister = DateTime.Now;
            planCompany.LastPayment = DateTime.Now;
            planCompany.IdCompany = company.Id;
            await planCompanyService.Create(planCompany);

            CostCenter costCenter = new CostCenter();
            costCenter.Name = "Padrão";
            costCenter.IdCompany = company.Id;
            await costCenterService.Create(costCenter);

            user.IdCompany = company.Id;
            user.Password = hash;
            user.VerifiedEmail = false;
            user.TokenVerify = Guid.NewGuid().ToString();
            await base.Create(user);
            EmailResponse emailResp = await emailService.SendVerificationEmailAsync(new EmailRequest
            {
                Email = user.Email,
                Name = user.Name,
                UserType = (int)user.TypeUser,

            }, user.TokenVerify);
            if (!emailResp.Success)
                return emailResp.Message;
            return "Salvo com Sucesso!";
        }
        public async Task<User> GetByToken(string token)
        {
            return await (repository as IUserRepository).GetByToken(token);
        }
        public async Task<string> ResetPassword(string email, string novaSenha, string confirmarSenha = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "Email é obrigatório!";

            if (string.IsNullOrWhiteSpace(novaSenha))
                return "Nova senha é obrigatória!";

            // Validação de confirmação de senha (se for fornecida)
            if (!string.IsNullOrWhiteSpace(confirmarSenha) && novaSenha != confirmarSenha)
                return "As senhas não conferem!";

            // Buscar usuário pelo email
            var authenticateModel = new AuthenticateModel { Email = email };
            var user = await (repository as IUserRepository).GetUser(authenticateModel);

            if (user == null)
                return "Usuário não encontrado!";

            // Validar força da senha
            if (novaSenha.Length < 6)
                return "A senha deve ter no mínimo 6 caracteres!";

            // Criptografar e salvar a nova senha
            Cryptography cryptography = new Cryptography();
            var novaSenhaHash = cryptography.addsEncrypted(novaSenha);

            user.Password = novaSenhaHash;
            //use = DateTime.Now;

            await base.Alter(user);

            return "Senha redefinida com sucesso!";
        }
        public async Task<ResponseGeneric> SaveCompanyUser(User user)
        {
            AuthenticateModel authenticateModel = new AuthenticateModel();
            authenticateModel.Email = user.Email;

            var userExist = await (repository as IUserRepository).GetUser(authenticateModel);
            if (userExist != null)
                return new ResponseGeneric {Success=false, Message="Usuário já possui cadastro!" };

            Cryptography cryptography = new Cryptography();
            var hash = cryptography.addsEncrypted(user.Password);

            user.IdCompany = user.IdCompany;
            user.Password = hash;
            user.VerifiedEmail = true;
            user.TokenVerify = Guid.NewGuid().ToString();
            await base.Create(user);
            return new ResponseGeneric { Success = true, Message = "Usuário salvo com sucesso!" };
        }
        public async Task<PagedResult<User>> GetUsersByCompany(Filters filters)
        {
             return await (repository as IUserRepository).GetUsersByCompany(filters);
        }
    }


    public interface IUserService : IBaseService<User>
    {
        Task<string> SaveUser(User user);
        Task<AuthenticateResponse> Authenticate(AuthenticateModel model);
        Task<User> GetByToken(string token);
        Task<string> ResetPassword(string email, string novaSenha, string confirmarSenha = null);
        Task<ResponseGeneric> SaveCompanyUser(User user);
        Task<PagedResult< User>> GetUsersByCompany(Filters filters);

    }

}
