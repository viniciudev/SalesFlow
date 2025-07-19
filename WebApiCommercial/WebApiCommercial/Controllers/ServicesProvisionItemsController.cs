using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Moves;
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
  public class ServicesProvisionItemsController : ControllerBase
  {
    private readonly IServicesProvisionItemsService servicesProvisionItemsService;
    public ServicesProvisionItemsController(IServicesProvisionItemsService servicesProvisionItemsService)
    {
      this.servicesProvisionItemsService = servicesProvisionItemsService;
    }
    // GET: api/<ServicesProvisionItemsController>
    [HttpGet]
    public IEnumerable<string> Get()
    {
      return new string[] { "value1", "value2" };
    }

    // GET api/<ServicesProvisionItemsController>/5
    [HttpGet("GedByIdServiceProvision")]
    public async Task<ActionResult<PagedResult<ServicesProvisionItems>>> Get([FromQuery] Filters filters)
    {
      var data = await servicesProvisionItemsService.GedByIdServiceProvisionPaged(filters);
      return Ok(data);
    }

    // POST api/<ServicesProvisionItemsController>
    [HttpPost]
    public async Task<ActionResult<dynamic>> Post([FromBody] ServicesProvisionItems model)
    {
      var data=await servicesProvisionItemsService.SaveItem(model);
      return Ok(data);
    }

    // PUT api/<ServicesProvisionItemsController>/5
    [HttpPut]
    public async Task<ActionResult<dynamic>> Put([FromBody] ServicesProvisionItems model)
    {
      await servicesProvisionItemsService.Alter(model);
      return Ok(true);

    }

    // DELETE api/<ServicesProvisionItemsController>/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<dynamic>> Delete(int id)
    {
      await servicesProvisionItemsService.DeleteAsync(id);
      return Ok(true);
    }
  }
}
