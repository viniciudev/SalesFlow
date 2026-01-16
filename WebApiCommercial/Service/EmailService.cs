using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Org.BouncyCastle.Asn1.Ocsp;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace Service
{
    public interface IEmailService
    {
        Task<EmailResponse> SendVerificationEmailAsync(EmailRequest request, string tokenVerify);
    }
    public class EmailRequest
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public int UserType { get; set; }
    }
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISendGridClient _sendGridClient;
        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _sendGridClient = new SendGridClient(_emailSettings.SendGridApiKey);
        }
        public string GenerateVerificationUrl(string token, string email)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return $"{baseUrl}/api/email/verify-email?token={token}&email={email}";
        }
        //public async Task<EmailResponse> SendVerificationEmailAsync(EmailRequest request,string verificationToken)
        //{

        //    try
        //    {
        //        var verificationUrl = GenerateVerificationUrl(verificationToken, request.Email);
        //        var emailBody = BuildEmailBody(request.Name, verificationUrl, request.UserType);

        //        var message = new MimeMessage();
        //        message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
        //        message.To.Add(new MailboxAddress(request.Name, request.Email));
        //        message.Subject = "Verificação de Email - Seu Sistema";

        //        var bodyBuilder = new BodyBuilder
        //        {
        //            HtmlBody = emailBody
        //        };
        //        message.Body = bodyBuilder.ToMessageBody();

        //        using (var client = new SmtpClient())
        //        {
        //            // Configuração baseada na porta
        //            if (_emailSettings.SmtpPort == 465)
        //            {
        //                // Porta 465 - SSL explícito
        //                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, SecureSocketOptions.SslOnConnect);
        //            }
        //            else if (_emailSettings.SmtpPort == 587)
        //            {
        //                // Porta 587 - STARTTLS
        //                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
        //            }
        //            else
        //            {
        //                // Auto-detect para outras portas
        //                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, SecureSocketOptions.Auto);
        //            }

        //            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
        //            await client.SendAsync(message);
        //            await client.DisconnectAsync(true);
        //        }

        //        _logger.LogInformation($"Email de verificação enviado para: {request.Email}");

        //        return new EmailResponse
        //        {
        //            Success = true,
        //            Message = "Email de verificação enviado com sucesso",
        //            SentDate = DateTime.UtcNow,

        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Erro ao enviar email para: {request.Email}");
        //        return new EmailResponse
        //        {
        //            Success = false,
        //            Message = $"Falha ao enviar email: {ex.Message}",
        //            SentDate = null
        //        };
        //    }

        //}
        public async Task<EmailResponse> SendVerificationEmailAsync(
        EmailRequest request,
        string verificationToken)
        {
            try
            {
                var verificationUrl = GenerateVerificationUrl(verificationToken, request.Email);
                var emailBody = BuildEmailBody(request.Name, verificationUrl, request.UserType);
                var plainTextContent = "Por favor, verifique seu email clicando no link fornecido.";

                var from = new EmailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
                var to = new EmailAddress(request.Email, request.Name);
                var subject = "Verificação de Email - Seu Sistema";

                var msg = MailHelper.CreateSingleEmail(
                    from,
                    to,
                    subject,
                    plainTextContent,
                    emailBody
                );

                var response = await _sendGridClient.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                   

                    return new EmailResponse
                    {
                        Success = true,
                        Message = "Email de verificação enviado com sucesso",
                        SentDate = DateTime.UtcNow
                    };
                }
                else
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    return new EmailResponse
                    {
                        Success = false,
                        Message = $"Falha no SendGrid. Status: {response.StatusCode}, Body: {errorBody}",
                        SentDate = null
                    };
                }
            }
            catch (Exception ex)
            {
              
                return new EmailResponse
                {
                    Success = false,
                    Message = $"Falha ao enviar email: {ex.Message}",
                    SentDate = null
                };
            }
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
                var token = Guid.NewGuid().ToString();
                var tokenHash = BCrypt.Net.BCrypt.HashPassword(token);

                // Salvar token no banco de dados (você precisa implementar isso no seu serviço)
                await userService.SaveResetPasswordToken(user.Id, tokenHash, DateTime.UtcNow.AddHours(1));

                // Construir URL de recuperação
                var resetUrl = $"{Request.Scheme}://{Request.Host}/forgot-password?token={token}&email={request.Email}";

                // Enviar email (usando seu EmailService)
                var emailRequest = new EmailRequest
                {
                    Email = request.Email,
                    Name = user.Name
                };

                // Você pode criar um método específico para emails de recuperação
                await SendResetPasswordEmail(emailRequest, resetUrl);

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
        private async Task SendResetPasswordEmail(EmailRequest request, string resetUrl)
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

            // Enviar usando SendGrid (ou seu método atual)
            var from = new EmailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
            var to = new EmailAddress(request.Email, request.Name);
            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                "Recuperação de Senha - StockFlow",
                "Para redefinir sua senha, clique no link: " + resetUrl,
                emailBody
            );

            var response = await _sendGridClient.SendEmailAsync(msg);
        }
        private string GenerateVerificationToken(string email)
        {
            return Guid.NewGuid().ToString();
        }

        private string BuildEmailBody(string name, string verificationUrl, int userType)
        {
            var userTypeDescription = userType switch
            {
                1 => "Fornecedor",
                2 => "Gestor",
                _ => "Usuário"
            };

            return $@"
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
                        <h1>Verificação de Email</h1>
                    </div>
                    <div class='content'>
                        <h2>Olá, {name}!</h2>
                        <p>Obrigado por se cadastrar como {userTypeDescription} em nosso sistema.</p>
                        <p>Para completar seu cadastro, por favor clique no botão abaixo para verificar seu email:</p>
                        <p style='text-align: center;'>
                            <a href='{verificationUrl}' class='button'>Verificar Email</a>
                        </p>
                        <p>Se o botão não funcionar, copie e cole o link abaixo no seu navegador:</p>
                        <p style='word-break: break-all;'>{verificationUrl}</p>
                        <p><strong>Este link expira em 24 horas.</strong></p>
                        <p>Se você não solicitou este cadastro, por favor ignore este email.</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2024 Seu Sistema. Todos os direitos reservados.</p>
                    </div>
                </div>
            </body>
            </html>";
        }
    }

    // Models/EmailResponse.cs
    public class EmailResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime? SentDate { get; set; }
    }
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public int Timeout { get; set; } = 10000;
        public string SendGridApiKey { get; set; } = string.Empty;
    }
}
