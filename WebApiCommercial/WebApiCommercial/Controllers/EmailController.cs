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

        //[HttpGet("verify-email")]
        //public async Task<IActionResult> VerifyEmail([FromQuery] string token, [FromQuery] string email)
        //{
        //    try
        //    {
        //        //mandar o id
        //        // Aqui você implementaria a lógica para verificar o token
        //        // Validar se o token é válido e não expirou
        //        // Atualizar o usuário como verificado no banco de dados
        //        User user = await _userService.GetByToken(token);
        //        if (user != null && user.Email == email)
        //        {
        //            user.VerifiedEmail = true;
        //            await _userService.Alter(user);

        //            // Exemplo simples:
        //            // var isValid = await ValidateVerificationTokenAsync(email, token);
        //            // if (!isValid) return BadRequest("Token inválido ou expirado");

        //            return Ok(new
        //            {
        //                //Success = true,
        //                Message = "Email verificado com sucesso",
        //                //VerifiedDate = DateTime.UtcNow
        //            });
        //        }
        //        else
        //        {
        //            return BadRequest("Falha ao verificar email.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erro na verificação de email");
        //        return StatusCode(500, "Erro na verificação");
        //    }
        //}
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token, [FromQuery] string email)
        {
            try
            {
                User user = await _userService.GetByToken(token);

                if (user != null && user.Email == email)
                {
                    user.VerifiedEmail = true;
                    await _userService.Alter(user);

                    return Content(@"
<!DOCTYPE html>
<html lang='pt-br'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Email Verificado</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }
        
        .container {
            background: white;
            border-radius: 15px;
            padding: 40px;
            text-align: center;
            box-shadow: 0 10px 30px rgba(0,0,0,0.2);
            max-width: 500px;
            width: 100%;
        }
        
        .success-icon {
            color: #4CAF50;
            font-size: 80px;
            margin-bottom: 20px;
        }
        
        h1 {
            color: #333;
            margin-bottom: 15px;
        }
        
        p {
            color: #666;
            margin-bottom: 30px;
            line-height: 1.6;
        }
        
        .btn {
            display: inline-block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 15px 40px;
            border-radius: 50px;
            text-decoration: none;
            font-weight: bold;
            font-size: 16px;
            transition: transform 0.3s, box-shadow 0.3s;
            border: none;
            cursor: pointer;
        }
        
        .btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(0,0,0,0.2);
        }
        
        .loading {
            display: none;
            font-size: 14px;
            color: #666;
            margin-top: 15px;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='success-icon'>✓</div>
        <h1>Email Verificado com Sucesso!</h1>
        <p>Sua conta foi ativada com sucesso. Agora você pode fazer login e aproveitar todos os recursos do nosso sistema.</p>
        <a href='https://studio-to69.onrender.com' class='btn' id='loginBtn'>Ir para Login</a>
        <p class='loading' id='loading'>Redirecionando em 5 segundos...</p>
    </div>
    
    <script>
        // Redirecionamento automático após 5 segundos
        let seconds = 5;
        const loadingElement = document.getElementById('loading');
        const countdownElement = document.getElementById('countdown');
        
        setTimeout(() => {
            loadingElement.style.display = 'block';
            
            const countdown = setInterval(() => {
                loadingElement.textContent = `Redirecionando em ${seconds} segundos...`;
                seconds--;
                
                if (seconds < 0) {
                    clearInterval(countdown);
                    window.location.href = 'https://studio-to69.onrender.com';
                }
            }, 1000);
        }, 1000);
        
        // Adicionar evento ao botão
        document.getElementById('loginBtn').addEventListener('click', function(e) {
            e.preventDefault();
            this.textContent = 'Redirecionando...';
            this.disabled = true;
            setTimeout(() => {
                window.location.href = 'https://studio-to69.onrender.com';
            }, 500);
        });
    </script>
</body>
</html>", "text/html");
                }
                else
                {
                    return Content(@"
<!DOCTYPE html>
<html lang='pt-br'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Erro na Verificação</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }
        
        .container {
            background: white;
            border-radius: 15px;
            padding: 40px;
            text-align: center;
            box-shadow: 0 10px 30px rgba(0,0,0,0.2);
            max-width: 500px;
            width: 100%;
        }
        
        .error-icon {
            color: #f44336;
            font-size: 80px;
            margin-bottom: 20px;
        }
        
        h1 {
            color: #333;
            margin-bottom: 15px;
        }
        
        p {
            color: #666;
            margin-bottom: 20px;
            line-height: 1.6;
        }
        
        .btn {
            display: inline-block;
            background: #f44336;
            color: white;
            padding: 12px 30px;
            border-radius: 50px;
            text-decoration: none;
            font-weight: bold;
            transition: background 0.3s;
            margin: 10px;
        }
        
        .btn:hover {
            background: #d32f2f;
        }
        
        .btn-secondary {
            background: #757575;
        }
        
        .btn-secondary:hover {
            background: #616161;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='error-icon'>✗</div>
        <h1>Erro na Verificação</h1>
        <p>O link de verificação é inválido ou expirou.</p>
        <p>Por favor, solicite um novo link de verificação ou entre em contato com o suporte.</p>
        <div>
            <a href='https://studio-to69.onrender.com' class='btn'>Ir para Login</a>
            <a href='/resend-verification' class='btn btn-secondary'>Reenviar Verificação</a>
        </div>
    </div>
</body>
</html>", "text/html");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na verificação de email");
                return Content(@"
<!DOCTYPE html>
<html lang='pt-br'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Erro no Servidor</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: #f5f5f5;
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }
        
        .container {
            background: white;
            border-radius: 10px;
            padding: 30px;
            text-align: center;
            box-shadow: 0 5px 15px rgba(0,0,0,0.1);
            max-width: 500px;
            width: 100%;
        }
        
        h1 {
            color: #f44336;
            margin-bottom: 15px;
        }
        
        .btn {
            display: inline-block;
            background: #2196F3;
            color: white;
            padding: 10px 25px;
            border-radius: 5px;
            text-decoration: none;
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <div class='container'>
        <h1>Erro no Servidor</h1>
        <p>Ocorreu um erro ao processar sua verificação. Por favor, tente novamente mais tarde.</p>
        <a href='/' class='btn'>Voltar para Home</a>
    </div>
</body>
</html>", "text/html");
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
