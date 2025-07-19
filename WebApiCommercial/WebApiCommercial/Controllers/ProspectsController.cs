using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO;
using Model.Registrations;
using Repository;
using Service;
using System;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiCommercial.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ProspectsController : ControllerBase
  {
    private readonly IProspectsService _prospectsService;
    public ProspectsController(IProspectsService _prospectsService)
    {
      this._prospectsService = _prospectsService;
    }
    // GET: api/<ProspectsController>
    [HttpGet]
    public async Task<ActionResult<PagedResult<Prospects>>> Get([FromQuery] Filters filters,
      [FromHeader] int tenantid)
    {
      filters.idCompany = tenantid;
      var data = await _prospectsService.GetAllPaged(filters);
      return Ok(data);
    }
    [HttpGet("GetByMonthAllProspects")]
    public async Task<ActionResult<SaleInfoResponse>> GetByMonthAllProspects([FromQuery] Filters filters,
   [FromHeader] int tenantid)
    {
      filters.idCompany = tenantid;
      return Ok(await _prospectsService.GetByMonthAllProspects(filters));
    }

    // GET api/<ProspectsController>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
      return "value";
    }

    // POST api/<ProspectsController>
    [HttpPost]
    public async Task<ActionResult<dynamic>> Post([FromBody] Prospects prospects,
      [FromHeader] int tenantid)
    {
      prospects.IdCompany = tenantid;
      prospects.RegisterDate = DateTime.Now;
      await _prospectsService.Create(prospects);
      return Ok(prospects);
    }

    // PUT api/<ProspectsController>/5
    [HttpPut]
    public async Task<ActionResult<dynamic>> Put([FromBody] Prospects prospects,
      [FromHeader] int tenantid)
    {

      await _prospectsService.Alter(prospects);
      //salvar e atualizar etapasProspect
      return Ok(true);
    }

    // DELETE api/<ProspectsController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}
