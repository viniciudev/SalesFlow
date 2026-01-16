using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO;
using Model.DTO.User;
using SendGrid.Helpers.Mail;
using Service;
using System;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace WebAppCommercial.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly ICompanyService companyService;

        public UserController(IUserService userService, ICompanyService companyService)
        {
            this.userService = userService;
            this.companyService = companyService;
        }
        // Adicione no UserController.cs
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Dados inválidos",
                        errors = ModelState.Values.SelectMany(v => v.Errors)
                    });
                }

                var user = await userService.GetUserByEmail(request.Email);
                if (user == null)
                {
                    // Por segurança, retornamos sucesso mesmo se o email não existir
                    return Ok(new
                    {
                        success = true,
                        message = "Se o email existir, enviaremos um link de recuperação"
                    });
                }

                // Gerar token de recuperação (expira em 1 hora)
                //var token = Guid.NewGuid().ToString();
                //var tokenHash = BCrypt.Net.BCrypt.HashPassword(token);

                // Salvar token no banco de dados (você precisa implementar isso no seu serviço)
                //await userService.SaveResetPasswordToken(user.Id, tokenHash, DateTime.UtcNow.AddHours(1));
                var urlweb = "https://studio-to69.onrender.com";
                // Construir URL de recuperação
                var resetUrl = $"{urlweb}/forgot-password?token={user.TokenVerify}&email={request.Email}";

                // Enviar email (usando seu EmailService)
                var emailRequest = new EmailRequest
                {
                    Email = request.Email,
                    Name = user.Name
                };

                // Você pode criar um método específico para emails de recuperação
                await userService.SendResetPasswordEmail(emailRequest, resetUrl);

                return Ok(new
                {
                    success = true,
                    message = "Link de recuperação enviado com sucesso"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro interno do servidor",
                    error = ex.Message
                });
            }
        }

        // Método para enviar email de recuperação
        

        [HttpGet]
        public async Task<ActionResult<User>> Get()
        {
            var result = await userService.GetAll();
            return Ok(result);
        }

        [HttpGet("getteste")]
        //[Authorize]
        public async Task<ActionResult<User>> GetTeste()
        {
            return Ok("Logado");
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<User> Get(int id)
        {
            var user = new User { Name = "ProfControl user" };
            return user;
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Dados inválidos",
                        errors = ModelState.Values.SelectMany(v => v.Errors)
                    });
                }

                var resultado = await userService.ResetPassword(request.Email, request.NovaSenha);

                if (resultado == "Senha redefinida com sucesso!")
                {
                    return Ok(new { message = resultado });
                }
                else
                {
                    return BadRequest(new { message = resultado });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro interno do servidor",
                    error = ex.Message
                });
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }


        [HttpPost]
        public async Task<ActionResult<dynamic>> Create([FromBody] User user)
        {
            var data = await userService.SaveUser(user);
            return Ok(data);
        }
        [HttpPost("usercompany")]
        public async Task<ActionResult> CompanyUser([FromHeader] int tenantid, [FromBody] User user)
        {
            user.IdCompany = tenantid;
            var data = await userService.SaveCompanyUser(user);
            return Ok(data);
        }
        [HttpPut("alterusercompany")]
        public async Task<ActionResult> AlterCompanyUser([FromHeader] int tenantid, [FromBody] User user)
        {
            user.IdCompany = tenantid;
            var data = await userService.AlterCompanyUser(user);
            return Ok(data);
        }

        [HttpGet("usersbycompany")]
        public async Task<IActionResult> GetUsersByCompany([FromHeader] int tenantid, [FromQuery] Filters filters)
        {
            filters.IdCompany = tenantid;
            var data = await userService.GetUsersByCompany(filters);
            return Ok(data);
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult> Authenticate(AuthenticateModel model)
        {
            var response = await userService.Authenticate(model);
            return Ok(response);
        }

    }
}
