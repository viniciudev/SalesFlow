using Microsoft.AspNetCore.Mvc;
using Model.Registrations;
using Service;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiCommercial.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class CostCenterController : ControllerBase
  {
    private readonly ICostCenterService costCenterService;
    public CostCenterController(ICostCenterService costCenterService)
    {
      this.costCenterService = costCenterService;
    }
    // GET: api/<CostCenterController>
    [HttpGet("GetByIdCompany")]
    public async Task<ActionResult<List<CostCenter>>> GetByIdCompany([FromHeader]int tenantid)
    {
      return Ok(await  costCenterService.GetByIdCompany(tenantid));
    }

    // GET api/<CostCenterController>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
      return "value";
    }

    // POST api/<CostCenterController>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/<CostCenterController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<CostCenterController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}
