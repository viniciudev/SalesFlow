using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Registrations;
using Repository;
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
  public class ServiceProvidedController : ControllerBase
  {

    private readonly IServiceProvidedService serviceProvidedService;

    public ServiceProvidedController(IServiceProvidedService serviceProvidedService)
    {
      this.serviceProvidedService = serviceProvidedService;
    }

    // GET: api/<ServiceProvidedController>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ServiceProvided>>> Get([FromQuery] Filters filters, [FromHeader] int tenantid)
    {
      filters.idCompany = tenantid;
      var data = await serviceProvidedService.GetAllPaged(filters);
      return Ok(data);
    }

    [HttpGet("GetListByName")]
    public async Task<ActionResult<List<ServiceProvided>>> GetListByName([FromQuery] Filters filter, [FromHeader] int tenantid)
    {
      filter.idCompany = tenantid;
      var data = await serviceProvidedService.GetListByName(filter);
      return Ok(data);
    }

    // GET api/<ServiceProvidedController>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
      return "value";
    }

    // POST api/<ServiceProvidedController>
    [HttpPost]
    public async Task<ActionResult<dynamic>> Post([FromBody] ServiceProvided model,
      [FromHeader] int tenantid)
    {
      model.IdCompany = tenantid;
      await serviceProvidedService.Save(model);
      return true;
    }

    // PUT api/<ServiceProvidedController>/5
    [HttpPut]
    public async Task<ActionResult<dynamic>> Put([FromBody] ServiceProvided model)
    {
      await serviceProvidedService.Alter(model);
      return true;
    }

    // DELETE api/<ServiceProvidedController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}
