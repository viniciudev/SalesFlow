using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO;
using Repository;
using Service;
using System;
using System.Threading.Tasks;

namespace WebAppCommercial.Controllers
{
  [Produces("application/json")]
  [Route("api/[controller]")]
  [ApiController]
  public class ClientController : ControllerBase
  {
    private readonly IClientService clientService;

    public ClientController(IClientService clientService)
    {
      this.clientService = clientService;

    }
    [HttpGet]
    public async Task<ActionResult<PagedResult<Client>>> Get([FromQuery] Filters filter,
      [FromHeader]int tenantid)
    {
      filter.idCompany = tenantid;
      var pagedData = await clientService.GetAllPaged(filter);

      return Ok(pagedData);
    }

    // GET api/<ClientController>/5
    [HttpGet("GetByMonthAllClients")]
    public async Task<ActionResult<ClientInfoResponse>> GetByMonthAllClients([FromQuery] Filters filter,
      [FromHeader]int tenantid)
    {
      filter.idCompany = tenantid;
      return Ok(await clientService.GetByMonthAllClients(filter));
    }

    // POST api/<ClientController>
    [HttpPost("{id}")]
    public void Post([FromBody] string value)
    {

    }
    [HttpPost]
    public async Task<ActionResult<dynamic>> Post([FromBody] Client model, [FromHeader] int tenantid)
    {
      model.IdCompany = tenantid;
      model.CreatDate= DateTime.Now;
      await clientService.Save(model);
      return Ok(true);
    }


    // PUT api/<ClientController>/5
    [HttpPut()]
    public async Task<ActionResult<dynamic>> Put([FromBody] Client model)
    {
    
          await clientService.Alter(model);

          return true;
      
    }

    // DELETE api/<ClientController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {

    }
  }
}
