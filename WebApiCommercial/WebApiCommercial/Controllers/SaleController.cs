using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO;
using Model.Moves;
using Repository;
using Service;
using System.Threading.Tasks;

namespace WebApiCommercial.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class SaleController : ControllerBase
	{
		private readonly ISaleService saleService;

		public SaleController(ISaleService saleService)
		{
			this.saleService = saleService;
		}
		// GET: api/<SaleController>
		[HttpGet]
		public async Task<ActionResult<PagedResult<Sale>>> GetPaged([FromQuery] Filters filters,
			[FromHeader] int tenantid)
		{
			filters.IdCompany = tenantid;
			return Ok(await saleService.GetAllPaged(filters));
		}

		[HttpGet("GetByMonthAllSales")]
		public async Task<ActionResult<SaleInfoResponse>> GetByMonthAllSales([FromQuery] Filters filters,
		[FromHeader] int tenantid)
		{
			filters.IdCompany = tenantid;
			return Ok(await saleService.GetByMonthAllSales(filters));
		}
		[HttpGet("GetByIdSale")]
		public async Task<ActionResult<Sale>> GetByIdSale([FromQuery] int id)
		{
			return Ok(await saleService.GetByIdSale(id));
		}
		[HttpGet("GetByWeekAllSales")]
		public async Task<ActionResult<SalesCommissionsInfo>> GetByWeekAllSales([FromQuery] Filters filters,
			[FromHeader] int tenantid)
		{
			filters.IdCompany = tenantid;
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
		public async Task<ActionResult<int>> PostWithItems([FromBody] SaleDto sale,
			[FromHeader] int tenantid)
		{
			sale.IdCompany = tenantid;
			int id = await saleService.SaveWithItems(sale);
			return Ok(id);
		}

		[HttpPut]
		public async Task<ActionResult<dynamic>> Put([FromBody] Sale sale,
			[FromHeader] int tenantid)
		{
			sale.IdCompany = tenantid;
			await saleService.Alter(sale);
			return Ok(true);
		}

		[HttpPut("PutWithItems")]
		public async Task<ActionResult<dynamic>> PutWithItems([FromBody] SaleDto sale,
			[FromHeader] int tenantid)
		{
			sale.IdCompany = tenantid;
			await saleService.PutWithItems(sale);
			return Ok(true);
		}

		// DELETE api/<SaleController>/5
		[HttpDelete("{id}")]
		public async Task<ActionResult> Delete(int id)
		{
			var resp=await saleService.Cancel(id);
			return Ok(resp);
		}

		// ===== NOVOS ENDPOINTS - PEDIDO DE VENDA =====

		[HttpPost("SalesOrder")]
		public async Task<ActionResult<int>> PostSalesOrder([FromBody] SaleDto sale,
			[FromHeader] int tenantid)
		{
			sale.IdCompany = tenantid;
			int id = await saleService.SaveSalesOrder(sale);
			return Ok(id);
		}

		[HttpPut("SalesOrder/{saleId}/Receive")]
		public async Task<ActionResult<ResponseGeneric>> ReceiveSalesOrder(int saleId,
			[FromBody] SaleDto receiveData,
			[FromHeader] int tenantid)
		{
			receiveData.IdCompany = tenantid;
			var resp = await saleService.ReceiveSalesOrder(saleId, receiveData);
			return Ok(resp);
		}
	}
}
