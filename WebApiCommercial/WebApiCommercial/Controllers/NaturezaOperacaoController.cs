

using Microsoft.AspNetCore.Mvc;
using Service;
using Service.Dtos;
using Service.Exceptions;
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
        public async Task<IActionResult> Post([FromHeader]int tenantid,[FromBody] NaturezaOperacaoCreateRequest request)
        {
            try
            {
                request.CompanyId = tenantid;
                var id = await _service.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = id }, new { id = id });
            }
            catch (DomainException dex)
            {
                return BadRequest(new { error = dex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] NaturezaOperacaoUpdateRequest request)
        {
            try
            {
                await _service.UpdateAsync(id, request);
                return NoContent();
            }
            catch (DomainException dex)
            {
                return BadRequest(new { error = dex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var list = await _service.GetAllAsync();
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
    }
}