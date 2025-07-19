using Microsoft.AspNetCore.Mvc;
using Model;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiCommercial.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class SearchClientController : ControllerBase
  {
    private readonly IClientService clientService;

    public SearchClientController(IClientService clientService)
    {
      this.clientService = clientService;

    }
    // GET: api/<SearchClientController>
    [HttpGet("contains")]
    public async Task<ActionResult<Client>> Get([FromHeader]Filters filters,[FromHeader]int tenantid )
    {
      filters.idCompany = tenantid;
      var data = await clientService.GetByName(filters);
      return Ok(data);
    }

    // GET api/<SearchClientController>/5
    [HttpGet]
    public async Task<ActionResult<List<Client>>> GetAll([FromHeader] Filters filters, [FromHeader] int tenantid)
    {
      filters.idCompany = tenantid;
      var data = await clientService.GetAllList(filters);
      return Ok(data);
    }

    // POST api/<SearchClientController>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/<SearchClientController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<SearchClientController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}
