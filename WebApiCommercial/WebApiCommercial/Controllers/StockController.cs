using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Moves;
using Service;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiCommercial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;
        public StockController(IStockService stockService) {
        _stockService = stockService;
        }    
        // GET: api/<StockController>
        [HttpGet("paged")]
        public async Task<ActionResult> Get([FromHeader]int tenantid, [FromBody] Filters filters )
        {
            try
            {
                var resp = await _stockService.GetAllPage(filters.pageNumber,filters.pageSize);
                return Ok(resp);
            }
            catch (System.Exception)
            {
                return BadRequest("Falha ao consultar moviemntos!");
            }
        }

    

        // POST api/<StockController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Stock value, [FromHeader] int tenantid)
        {
            try
            {
                value.IdCompany=tenantid;
                await _stockService.Create(value);
                return Ok("Salvo com sucesso!");
            }
            catch (System.Exception ex)
            {
                return BadRequest("Erro:"+ ex.Message);
           
            }
        }

       
    }
}
