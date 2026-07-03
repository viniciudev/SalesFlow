using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service;
using Service.Dtos;
using Service.Exceptions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApiCommercial.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NaturezaOperacaoController : ControllerBase
    {
        private readonly INaturezaOperacaoService _service;

        public NaturezaOperacaoController(INaturezaOperacaoService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromHeader] int tenantid, [FromBody] NaturezaOperacaoCreateRequest request)
        {
            try
            {
                request.CompanyId = tenantid;
                var id = await _service.CreateAsync(request);
                return Ok(new ResponseGeneric { Success = true });
            }
            catch (DomainException dex)
            {
                return Ok(new ResponseGeneric { Message = dex.Message, Success = false });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] NaturezaOperacaoUpdateRequest request)
        {
            try
            {
                await _service.UpdateAsync(id, request);
                return Ok(new ResponseGeneric { Success = true });
            }
            catch (DomainException dex)
            {
                return Ok(new ResponseGeneric { Message = dex.Message, Success = false });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromHeader] int tenantid)
        {
            var list = await _service.GetAllAsync(tenantid);
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (DomainException dex)
            {
                return BadRequest(new { error = dex.Message });
            }
        }

        // ========== MELHORIA 2: Batch de RegrasFiscais ==========
        [HttpPut("{id:int}/regrasfiscais/batch")]
        public async Task<IActionResult> AtualizarRegrasFiscaisBatch(int id, [FromBody] List<RegraFiscalDto> regras)
        {
            try
            {
                await _service.AtualizarRegrasFiscaisBatchAsync(id, regras);
                return Ok(new ResponseGeneric { Success = true, Message = "Matriz tributaria atualizada com sucesso." });
            }
            catch (DomainException dex)
            {
                return Ok(new ResponseGeneric { Message = dex.Message, Success = false });
            }
        }

        // ========== MELHORIA 4: Clonar matriz ==========
        [HttpPost("{origemId:int}/clonar-matriz/{destinoId:int}")]
        public async Task<IActionResult> ClonarMatriz(int origemId, int destinoId)
        {
            try
            {
                await _service.ClonarMatrizAsync(origemId, destinoId);
                return Ok(new ResponseGeneric { Success = true, Message = "Matriz tributaria clonada com sucesso." });
            }
            catch (DomainException dex)
            {
                return Ok(new ResponseGeneric { Message = dex.Message, Success = false });
            }
        }

        // ========== MELHORIA 4: Exportar CSV ==========
        [HttpGet("{id:int}/exportar-csv")]
        public async Task<IActionResult> ExportarCsv(int id)
        {
            try
            {
                var csv = await _service.ExportarMatrizCsvAsync(id);
                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                return File(bytes, "text/csv", "matriz-tributaria-" + id + ".csv");
            }
            catch (DomainException dex)
            {
                return BadRequest(new { error = dex.Message });
            }
        }

        // ========== MELHORIA 6: Migrar produtos ==========
        [HttpPost("migrar-produtos")]
        public async Task<IActionResult> MigrarProdutos([FromHeader] int tenantid)
        {
            try
            {
                var resultado = await _service.MigrarProdutosParaMatrizAsync(tenantid);
                return Ok(new ResponseGeneric { Success = true, Data = resultado });
            }
            catch (DomainException dex)
            {
                return Ok(new ResponseGeneric { Message = dex.Message, Success = false });
            }
        }
    }
}
