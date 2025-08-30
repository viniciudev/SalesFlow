using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IEmailService
    {
        Task<EmailResponse> SendVerificationEmailAsync(EmailRequest request);
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

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<EmailResponse> SendVerificationEmailAsync(EmailRequest request)
        {
            try
            {
                // Validar configurações
                if (string.IsNullOrEmpty(_emailSettings.SmtpHost) ||
                    string.IsNullOrEmpty(_emailSettings.Username) ||
                    string.IsNullOrEmpty(_emailSettings.Password))
                {
                    throw new InvalidOperationException("Configurações de email não foram definidas corretamente");
                }

                // Gerar token de verificação
                var verificationToken = GenerateVerificationToken(request.Email);

                // URL de verificação (ajuste conforme sua aplicação)
                var verificationUrl = $"https://seu-dominio.com/verify-email?token={verificationToken}&email={request.Email}";

                // Construir o corpo do email
                var emailBody = BuildEmailBody(request.Name, verificationUrl, request.UserType);

                using (var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort))
                {
                    client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                    client.EnableSsl = _emailSettings.EnableSsl;
                    client.Timeout = _emailSettings.Timeout;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                        Subject = "Verificação de Email - Seu Sistema",
                        Body = emailBody,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(request.Email);

                    await client.SendMailAsync(mailMessage);
                }

                _logger.LogInformation($"Email de verificação enviado para: {request.Email}");

                return new EmailResponse
                {
                    Success = true,
                    Message = "Email de verificação enviado com sucesso",
                    SentDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar email para: {request.Email}");

                return new EmailResponse
                {
                    Success = false,
                    Message = $"Falha ao enviar email: {ex.Message}",
                    SentDate = null
                };
            }
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
    }
}
