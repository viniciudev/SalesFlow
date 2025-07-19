using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Moves;
using Repository;
using Service;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiCommercial.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class SaleItemsController : ControllerBase
  {
    private readonly ISaleItemsService saleItemsService;
    public SaleItemsController(ISaleItemsService saleItemsService)
    {
      this.saleItemsService = saleItemsService;
    }
    // GET: api/<SaleItemsController>
    [HttpGet]
    public async Task<ActionResult<PagedResult<SaleItems>>> Get([FromQuery] Filters filters)
    {
      return  Ok(await saleItemsService.GetPaged(filters));
    }

    // GET api/<SaleItemsController>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
      return "value";
    }

    // POST api/<SaleItemsController>
    [HttpPost]
    public async Task<ActionResult<dynamic>> Post([FromBody] SaleItems saleItems,
      [FromHeader] int tenantid)
    {
      await saleItemsService.SaveSaleItems(saleItems,tenantid);
      return Ok(true);
    }

    // PUT api/<SaleItemsController>/5
    [HttpPut]
    public async Task<ActionResult<dynamic>> Put([FromBody] SaleItems saleItems,
      [FromHeader]int tenantid)
    {
      await saleItemsService.AlterSaleItems(saleItems,tenantid);
      return Ok(true);
    }

    // DELETE api/<SaleItemsController>/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<dynamic>> Delete(int id)
    {
      await saleItemsService.Delete(id);
      return Ok(true);
    }
  }
}
