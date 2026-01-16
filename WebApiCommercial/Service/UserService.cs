using Microsoft.IdentityModel.Tokens;
using Model;
using Model.DTO;
using Model.Moves;
using Model.Registrations;
using Repository;
using SendGrid.Helpers.Mail;
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
        public async  Task<ResponseGeneric> AlterCompanyUser(User user)
        {
            User user1 = await base.GetByIdAsync(user.Id);
            user1.CellPhone = user.CellPhone;
            user1.BirthDate = user.BirthDate;
            user1.Name = user.Name;
            user1.Email = user.Email;
            if(!string.IsNullOrEmpty(user.Password))
            {
                Cryptography cryptography = new Cryptography();
                var hash = cryptography.addsEncrypted(user.Password);
                user1.Password = hash;
            }
            await base.Alter(user1);
            return new ResponseGeneric
            {
                Success = true,
                Message = "Alterado com sucesso!"
            };
        }
       

        public async Task SendResetPasswordEmail(EmailRequest request, string resetUrl)
        {
            var emailBody = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8'>
        <style>
            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
            .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
            .content {{ background-color: #f8f9fa; padding: 30px; }}
            .button {{ background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; }}
            .footer {{ text-align: center; margin-top: 20px; color: #6c757d; }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <h1>Recuperação de Senha</h1>
            </div>
            <div class='content'>
                <h2>Olá, {request.Name}!</h2>
                <p>Recebemos uma solicitação para redefinir sua senha.</p>
                <p>Clique no botão abaixo para criar uma nova senha:</p>
                <p style='text-align: center;'>
                    <a href='{resetUrl}' class='button'>Redefinir Senha</a>
                </p>
                <p>Se o botão não funcionar, copie e cole o link abaixo no seu navegador:</p>
                <p style='word-break: break-all;'>{resetUrl}</p>
                <p><strong>Este link expira em 1 hora.</strong></p>
                <p>Se você não solicitou a recuperação de senha, por favor ignore este email.</p>
            </div>
            <div class='footer'>
                <p>&copy; 2024 StockFlow. Todos os direitos reservados.</p>
            </div>
        </div>
    </body>
    </html>";
            await emailService.SendResetPasswordEmailAsync(request, emailBody, resetUrl);
          
        }
        public async Task<User> GetUserByEmail(string email)
        {
            User user = await (repository as IUserRepository).GetUserByEmail(email);
            return user;
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
        Task<ResponseGeneric> AlterCompanyUser(User user);

     
        Task<User> GetUserByEmail(string email);
      
        Task SendResetPasswordEmail(EmailRequest request, string resetUrl);
    }

}
