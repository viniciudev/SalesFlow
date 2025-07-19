using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Moves;
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
  public class ServicesProvisionController : ControllerBase
  {

    private IServicesProvisionService servicesProvisionService;

    public ServicesProvisionController(IServicesProvisionService servicesProvisionService)
    {
      this.servicesProvisionService = servicesProvisionService;
    }
    // GET: api/<ServicesProvisionController>
    [HttpGet]
    public async Task<ActionResult<ServicesProvision>> Get([FromQuery]Filters filters,[FromHeader]int tenantid)
    {
      filters.idCompany = tenantid;
      var data= await servicesProvisionService.GetAllServicesProvisionPaged(filters);
      return Ok(data);
    }

    // GET api/<ServicesProvisionController>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
      return "value";
    }

    // POST api/<ServicesProvisionController>
    [HttpPost]
    public async  Task<ActionResult<dynamic>> Post([FromBody] ServicesProvision model,[FromHeader]int tenantid)
    {
      model.IdCompany = tenantid;
      await servicesProvisionService.SaveService(model);
      return Ok(model);
    }

    // PUT api/<ServicesProvisionController>/5
    [HttpPut]
    public async Task<ActionResult<dynamic>> Put([FromBody] ServicesProvision servicesProvision, [FromHeader] int tenantid)
    {
      servicesProvision.IdCompany = tenantid;
      await servicesProvisionService.Alter(servicesProvision);
      return Ok(true);
    }

    // DELETE api/<ServicesProvisionController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}
