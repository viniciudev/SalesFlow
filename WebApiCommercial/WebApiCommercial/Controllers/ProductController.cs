using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO;
using Model.Registrations;
using Repository;
using Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiCommercial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {

        private readonly IProductService productServie;
        private readonly ITributacaoResolverService _tributacaoResolver;

        public ProductController(IProductService productServie, ITributacaoResolverService tributacaoResolver)
        {
            this.productServie = productServie;
            _tributacaoResolver = tributacaoResolver;
        }

        // GET: api/<ProductController>
        [HttpGet]
        public async Task<ActionResult<PagedResult<Product>>> Get([FromQuery] Filters filter, [FromHeader] int tenantid)
        {
            filter.IdCompany = tenantid;
            var data = await productServie.GetAllPaged(filter);
            return Ok(data);
        }

        // GET api/<ProductController>/5
        [HttpGet("GetListByName")]
        public async Task<ActionResult<List<Product>>> GetListByName([FromQuery] Filters filter, [FromHeader] int tenantid)
        {
            filter.IdCompany = tenantid;
            var data = await productServie.GetListByName(filter);
            return Ok(data);
        }

        // POST api/<ProductController>
        [HttpPost]
        public async Task<ActionResult<dynamic>> Post([FromBody] ProductCreateModelDto model, [FromHeader] int tenantid)
        {
            try
            {
                await productServie.SaveProduct(model, tenantid);
                return Ok(new { success = true, message = "Produto cadastrado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // PUT api/<ProductController>/5
        [HttpPut]
        public async Task<ActionResult<dynamic>> Put([FromBody] Product model)
        {
            try
            {

                await productServie.Alter(model);
            }
            catch (Exception ex)
            {

                throw;
            }

            return true;
        }

        // DELETE api/<ProductController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await productServie.DeleteAsync(id);
                return Ok(new { success = true, message = "Produto excluído com sucesso" });

            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Não é possível excluir o produto!" });
            }
        }

        // ===== NOVOS ENDPOINTS: TRIBUTAÇÃO POR PRODUTO =====

        /// <summary>
        /// Atualiza apenas a configuração tributária de um produto
        /// </summary>
        [HttpPut("{productId}/tributacao")]
        public async Task<IActionResult> AtualizarTributacao(int productId, [FromBody] AtualizarTributacaoRequest request)
        {
            try
            {
                await productServie.AtualizarTributacaoProdutoAsync(productId, request.ConfiguracaoTributaria, request.NaturezaOperacaoOrigemId);
                return Ok(new { success = true, message = "Tributação do produto atualizada com sucesso" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Remove a tributação própria do produto (volta a herdar da natureza)
        /// </summary>
        [HttpDelete("{productId}/tributacao")]
        public async Task<IActionResult> RemoverTributacao(int productId)
        {
            try
            {
                await productServie.RemoverTributacaoProdutoAsync(productId);
                return Ok(new { success = true, message = "Tributação própria removida. Produto usará configuração da natureza." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Clona a configuração tributária de uma natureza de operação
        /// </summary>
        [HttpGet("tributacao/clonar-de-natureza/{naturezaId}")]
        public async Task<IActionResult> ClonarTributacaoDeNatureza(int naturezaId)
        {
            try
            {
                var config = await _tributacaoResolver.ClonarDeNaturezaAsync(naturezaId);
                return Ok(new { success = true, data = config });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Request para atualizar tributação de um produto
    /// </summary>
    public class AtualizarTributacaoRequest
    {
        public ProductTributacaoDto ConfiguracaoTributaria { get; set; } = new();
        public int? NaturezaOperacaoOrigemId { get; set; }
    }
}
