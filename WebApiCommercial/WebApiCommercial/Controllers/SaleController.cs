using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO;
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
  public class SaleController : ControllerBase
  {
    private readonly ISaleService saleService;

    public SaleController(ISaleService saleService)
    {
      this.saleService = saleService;
    }
    // GET: api/<SaleController>
    [HttpGet]
    public async Task<ActionResult<PagedResult<Sale>>> GetPaged([FromQuery]Filters filters,
      [FromHeader]int tenantid)
    {
      filters.idCompany = tenantid;
      return Ok(await saleService.GetAllPaged(filters));
    }
    [HttpGet("GetByMonthAllSales")]
    public async Task<ActionResult<SaleInfoResponse>> GetByMonthAllSales([FromQuery] Filters filters,
    [FromHeader] int tenantid)
    {
      filters.idCompany = tenantid;
      return Ok(await saleService.GetByMonthAllSales(filters));
    }
    [HttpGet("GetByIdSale")]
    public async Task<ActionResult<Sale>> GetByIdSale([FromQuery]int id)
    {
      return Ok(await saleService.GetByIdSale(id));
    }
    [HttpGet("GetByWeekAllSales")]
    public async Task<ActionResult<SalesCommissionsInfo>> GetByWeekAllSales([FromQuery] Filters filters,
      [FromHeader] int tenantid)
    {
      filters.idCompany = tenantid;
      return Ok(await saleService.GetByWeekAllSales(filters));
    }
    [HttpGet("GetSalesmanByWeek")]
    public async Task<ActionResult<SalesCommissionsInfo>> GetSalesmanByWeek(
      [FromHeader] int tenantid)
    {
      return Ok(await saleService.GetSalesmanByWeek(tenantid));
    }
    // POST api/<SaleController>
    [HttpPost("PostWithItems")]
    public async Task<ActionResult<int>> PostWithItems([FromBody] Sale sale,
      [FromHeader]int tenantid)
    {
      sale.IdCompany = tenantid;
      int id=await saleService.SaveWithItems(sale);
      return Ok(id);
    }
    [HttpPut]
    public async Task<ActionResult<dynamic>> Post([FromBody] Sale sale,
      [FromHeader] int tenantid)
    {
      sale.IdCompany = tenantid;
       await saleService.Alter(sale);
      return Ok(true);
    }

    // PUT api/<SaleController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<SaleController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}
