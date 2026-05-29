using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO;
using Model.Moves;
using Repository;
using Service;
using System;
using System.Threading.Tasks;

namespace WebApiCommercial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ComprasController : ControllerBase
    {
        private readonly IPurchaseService purchaseService;

        public ComprasController(IPurchaseService purchaseService)
        {
            this.purchaseService = purchaseService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<Purchase>>> GetPaged([FromQuery] Filters filters,
            [FromHeader] int tenantid)
        {
            filters.IdCompany = tenantid;
            return Ok(await purchaseService.GetAllPaged(filters));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Purchase>> GetById(int id)
        {
            var compra = await purchaseService.GetByIdWithItems(id);
            if (compra == null)
                return NotFound();
            return Ok(compra);
        }

        [HttpPost("PostWithItems")]
        public async Task<ActionResult<int>> PostWithItems([FromBody] PurchaseDto purchase,
            [FromHeader] int tenantid)
        {
            try
            {
                purchase.IdCompany = tenantid;
                int id = await purchaseService.SaveWithItems(purchase);
                return Ok(id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erro interno ao salvar compra.", details = ex.Message });
            }
        }

        [HttpPut("PutWithItems")]
        public async Task<ActionResult> PutWithItems([FromBody] PurchaseDto purchase,
            [FromHeader] int tenantid)
        {
            try
            {
                purchase.IdCompany = tenantid;
                await purchaseService.UpdateWithItems(purchase);
                return Ok(true);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Erro interno ao atualizar compra.", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await purchaseService.DeleteAsync(id);
            return Ok(true);
        }
    }
}
