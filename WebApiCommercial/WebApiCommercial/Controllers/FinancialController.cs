using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO;
using Model.Moves;
using Repository;
using Service;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiCommercial.Controllers
{
  [Route("api/[controller]")]
  [ApiController]

  public class FinancialController : ControllerBase
  {
    private readonly IFinancialService financialService;
    public FinancialController(IFinancialService financialService)
    {
      this.financialService = financialService;
    }
    // GET: api/<FinancialController>
    [HttpGet("GetCommissions")]
    public async Task<ActionResult<PagedResult<Financial>>> GetCommissions([FromQuery] Filters filters
      , [FromHeader] int tenantid)
    {
      filters.IdCompany = tenantid;
      var data = await financialService.GetPagedByFilter(filters);
      return Ok(data);
    }

    [HttpGet("GetByIdCompany")]
    public async Task<ActionResult<PagedResult<Financial>>> GetByIdCompany([FromQuery] Filters filters
      , [FromHeader] int tenantid)
    {
      filters.IdCompany = tenantid;
      var data = await financialService.GetByIdCompany(filters);
      return Ok(data);
    }

    [HttpGet("GetByMonthAllCommission")]
    public async Task<ActionResult<CommissionInfoResponse>> GetByMonthAllCommission([FromQuery] Filters filters,
   [FromHeader] int tenantid)
    {
      filters.IdCompany = tenantid;
      return Ok(await financialService.GetByMonthAllCommission(filters));
    }
    // GET api/<FinancialController>/5
    [HttpGet("paged")]
    public async Task<ActionResult> GetPaged([FromQuery] Filters filters, [FromHeader] int tenantid)

    {
            filters.IdCompany = tenantid;
            return Ok(await financialService.GetPaged(filters));
    }

    // POST api/<FinancialController>
    [HttpPost]
    public async Task<ActionResult<dynamic>> Post([FromBody] FinancialRequest financial
      , [FromHeader] int tenantid)
    {
            try
            {
                financial.IdCompany = tenantid;
                bool resp=await financialService.CreateFinancial(financial);
                return Ok("Salvo com sucesso!");
            }
            catch (System.Exception ex)
            {

                return BadRequest(ex.Message);
            }

            
    }

    // PUT api/<FinancialController>/5
    [HttpPut]
    public async Task<ActionResult<dynamic>> Put([FromBody] Financial financial
      , [FromHeader] int tenantid)
    {
      await financialService.AlterFinancial(financial);
      return Ok(true);
    }

    // DELETE api/<FinancialController>/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<dynamic>> Delete(int id)
    {
      await financialService.DeleteAsync(id);
      return Ok(true);
    }
  }
}
