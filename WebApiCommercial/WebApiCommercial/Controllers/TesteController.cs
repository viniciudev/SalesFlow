using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service;
using System;
using System.Threading.Tasks;

namespace WebApiCommercial.Controllers
{
    // Adicione este endpoint para testar
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly DbContext _context;

        public TestController(IEmailService emailService, DbContext context)
        {
            _emailService = emailService;
            _context = context;
        }

        [HttpGet("database")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                return Ok(new
                {
                    Connected = canConnect,
                    Time = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message,
                    Details = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("email")]
        public async Task<IActionResult> TestEmail()
        {
            var testRequest = new EmailRequest
            {
                Name = "Test User",
                Email = "seu-email-de-teste@gmail.com",
                UserType = 0
            };

            var response = await _emailService.SendVerificationEmailAsync(
                testRequest,
                "test-token-123"
            );

            return Ok(response);
        }
    }
}
