using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Registrations;
using Repository;
using Service;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiCommercial.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class CommissionController : ControllerBase
  {
    private readonly ICommissionService commissionService;
    public CommissionController(ICommissionService commissionService)
    {
      this.commissionService = commissionService;
    }
    // GET: api/<CommissionController>
    [HttpGet("GetAllPagedProductsCommission")]
    public async Task<ActionResult<PagedResult<Commission>>> GetAllPagedProductsCommission(
      [FromQuery] Filters filters, [FromHeader]int tenantid)
    {
      filters.idCompany = tenantid;
      return Ok(await commissionService.GetAllPagedProductsCommission(filters));
    }
    [HttpGet("GetAllPagedServicesCommission")]
    public async Task<ActionResult<PagedResult<Commission>>> GetAllPagedServicesCommission(
      [FromQuery] Filters filters, [FromHeader] int tenantid)
    {
      filters.idCompany = tenantid;
      return Ok(await commissionService.GetAllPagedServicesCommission(filters));
    }
    // GET api/<CommissionController>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
      return "value";
    }

    // POST api/<CommissionController>
    [HttpPost("PostList")]
    public async Task<ActionResult<dynamic>> Post([FromBody] List<Commission> commissions,
      [FromHeader]int tenantid)
    {
      await commissionService.SaveListCommission(commissions);
      return Ok(true);
    }

    [HttpPost]
    public async Task<ActionResult<dynamic>> Post([FromBody] Commission commissions,
    [FromHeader] int tenantid)
    {
      var data=await commissionService.SaveCommission(commissions);
      return Ok(data);
    }

    // PUT api/<CommissionController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<CommissionController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}
