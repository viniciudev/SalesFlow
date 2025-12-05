using Microsoft.AspNetCore.Mvc;
using Model;
using Service;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiCommercial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        // GET: api/<DashboardController>
        [HttpGet("sales-comparison")]
        public async Task<ActionResult> Get([FromHeader] int tenantid)
        {
            return Ok(await _dashboardService.GetValeuSalesByIdCompany(tenantid));
        }
        [HttpGet("client-comparison")]
        public async Task<ActionResult> GetClient([FromHeader] int tenantid)
        {
            return Ok(await _dashboardService.GetAmountClientByIdCompany(tenantid));
        }
        [HttpGet("low-stock")]
        public async Task<ActionResult> GetStock([FromHeader] int tenantid)
        {
            return Ok(await _dashboardService.GetLowStockByIdCompany(tenantid));
        }
        [HttpGet("count-Products")]
        public async Task<ActionResult> GetCountProduct([FromHeader] int tenantid)
        {
            return Ok(await _dashboardService.GetProductsByIdCompany(tenantid));
        }

    }
}
