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
  public class SalesmanController : ControllerBase
  {
    private readonly ISalesmanService salesmanService;
    public SalesmanController(ISalesmanService salesmanService)
    {
      this.salesmanService = salesmanService;
    }
    // GET: api/<SalesmanController>
    [HttpGet]
    public async Task<ActionResult<PagedResult<Salesman>>> Get([FromQuery] Filters filter,
      [FromHeader] int tenantid)
    {
      filter.idCompany = tenantid;
      var data = await salesmanService.GetAllPaged(filter);
      return Ok(data);
    }

    // GET api/<SalesmanController>/5
    [HttpGet("GetListByName")]
    public async Task<ActionResult<List<Salesman>>> GetListByName([FromQuery] Filters filter,
      [FromHeader] int tenantid)
    {
      filter.idCompany = tenantid;
      var data = await salesmanService.GetListByName( filter);
      return Ok(data);
    }
    [HttpGet("GetAllByGuid")]
    public async Task<ActionResult<List<Salesman>>> GetAllByGuid([FromQuery] string guid)
    {      var data = await salesmanService.GetAllByGuid(guid);
      return Ok(data);
    }

    [HttpGet("GetAllList")]
    public async Task<ActionResult<List<Salesman>>> GetAllList([FromQuery] Filters filter,
    [FromHeader] int tenantid)
    {
      filter.idCompany = tenantid;
      var data = await salesmanService.GetAllList(filter);
      return Ok(data);
    }

    // POST api/<SalesmanController>
    [HttpPost]
    public async Task<ActionResult<dynamic>> Post([FromBody] Salesman model,
      [FromHeader] int tenantid)
    {
      model.IdCompany = tenantid;
      await salesmanService.SaveSalesman(model);
      return Ok(model);
    }

    // PUT api/<SalesmanController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<SalesmanController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}
