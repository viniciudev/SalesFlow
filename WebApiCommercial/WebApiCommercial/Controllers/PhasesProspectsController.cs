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
  public class PhasesProspectsController : ControllerBase
  {
    private readonly IPhasesProspectsService _phasesProspectsService;

    public PhasesProspectsController(IPhasesProspectsService _phasesProspectsService)
    {
      this._phasesProspectsService= _phasesProspectsService;
    }
    // GET: api/<PhasesProspectsController>
    [HttpGet]
    public async Task<ActionResult<List<PhasesProspects>>> Get([FromQuery]int idProspect)
    {
      var data= await _phasesProspectsService.GetList(idProspect);
      return Ok(data);
    }

    // GET api/<PhasesProspectsController>/5
    //[HttpGet("{id}")]
    //public string Get(int id)
    //{
    //  return "value";
    //}

    // POST api/<PhasesProspectsController>
    [HttpPost]
    public async Task<ActionResult<dynamic>> Post([FromBody] PhasesProspects phasesProspects)
    {
      await _phasesProspectsService.Create(phasesProspects);
      return Ok(true);
    }

    // PUT api/<PhasesProspectsController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {

    }

    // DELETE api/<PhasesProspectsController>/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<dynamic>> Delete(int id)
    {
      await _phasesProspectsService.DeleteAsync(id);
      return Ok(true);
    }
  }
}
