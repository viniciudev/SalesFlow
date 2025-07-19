using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Closure;
using Model.DTO;
using Repository;
using Service;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiCommercial.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ClosuresController : ControllerBase
  {
    // GET: api/<ClosuresController>
    private IClosuresService closuresService;
    public  ClosuresController(IClosuresService closuresService)
    {
      this.closuresService = closuresService;
    }
    [HttpGet]
    public async  Task<ActionResult<Closures>> GetCheckin([FromQuery]int idsalesman)
    {
      return  Ok(await closuresService.GetCheckin(idsalesman));
    }

    // GET api/<ClosuresController>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
      return "value";
    }
		[HttpGet("DrivingDistancebyAddressAndLngLat")]
		public async Task<ActionResult<Closures>> DrivingDistancebyAddressAndLngLat()
		{
      decimal value=await closuresService.DrivingDistancebyAddressAndLngLat("","","","");
			return Ok(value);
		}

		[HttpGet("GetByPaged")]
		public async Task<ActionResult<PagedResult<ClosuresResponse>>> GetByPaged([FromQuery]Filters filters )
		{
			var data=await closuresService.Getpaged(filters);
			return Ok(data);
		}
		// POST api/<ClosuresController>
		[HttpPost]
    public async Task<ActionResult<dynamic>> Post([FromBody] Closures value)
    {
      
      await closuresService.Save(value);
      return Ok(value);
    }

    // PUT api/<ClosuresController>/5
    [HttpPut]
    public async Task<ActionResult<dynamic>> Put([FromBody] Closures closures)
    {
      await closuresService.SaveCheckout(closures);
      return Ok("OK");
    }


    // DELETE api/<ClosuresController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}
