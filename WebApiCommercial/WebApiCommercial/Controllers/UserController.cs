using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO;
using Service;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        [HttpGet("GetUsersByCompany")]
        public async Task<ActionResult> GetUsersByCompany([FromHeader] int tenantid, [FromBody] Filters filters)
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
