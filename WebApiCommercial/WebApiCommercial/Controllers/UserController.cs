using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using Service;
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

    // PUT api/values/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {                                
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

    [HttpPost("authenticate")]
    public async Task<ActionResult> Authenticate(AuthenticateModel model)
    {
      var response = await userService.Authenticate(model);
      return Ok(response);
    }

  }
}
