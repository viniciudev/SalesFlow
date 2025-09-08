using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model;
using Service;
using System;
using System.Threading.Tasks;

namespace WebApiCommercial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailController> _logger;
        private readonly IUserService _userService;

        public EmailController(IEmailService emailService, ILogger<EmailController> logger, IUserService userService)
        {
            _emailService = emailService;
            _logger = logger;
            _userService = userService;
        }

        [HttpPost("send-verification")]
        public async Task<IActionResult> SendVerificationEmail([FromBody] EmailRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new EmailResponse
                    {
                        Success = false,
                        Message = "Dados de entrada inválidos"
                    });
                }

                // Validar email
                if (!IsValidEmail(request.Email))
                {
                    return BadRequest(new EmailResponse
                    {
                        Success = false,
                        Message = "Email inválido"
                    });
                }

                var result = await _emailService.SendVerificationEmailAsync(request, Guid.NewGuid().ToString());

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(500, result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint de envio de email");

                return StatusCode(500, new EmailResponse
                {
                    Success = false,
                    Message = "Erro interno do servidor"
                });
            }
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token, [FromQuery] string email)
        {
            try
            {
                //mandar o id
                // Aqui você implementaria a lógica para verificar o token
                // Validar se o token é válido e não expirou
                // Atualizar o usuário como verificado no banco de dados
                User user = await _userService.GetByToken(token);
                if (user != null && user.Email == email)
                {
                    user.VerifiedEmail = true;
                    await _userService.Alter(user);

                    // Exemplo simples:
                    // var isValid = await ValidateVerificationTokenAsync(email, token);
                    // if (!isValid) return BadRequest("Token inválido ou expirado");

                    return Ok(new
                    {
                        //Success = true,
                        Message = "Email verificado com sucesso",
                        //VerifiedDate = DateTime.UtcNow
                    });
                }
                else
                {
                    return BadRequest("Falha ao verificar email.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na verificação de email");
                return StatusCode(500, "Erro na verificação");
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
