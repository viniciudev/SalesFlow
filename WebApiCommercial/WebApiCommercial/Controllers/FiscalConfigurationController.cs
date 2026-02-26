
using Microsoft.AspNetCore.Mvc;
using Service;
using Model.Registrations;
using System.Threading.Tasks;

namespace WebApiCommercial.Controllers
{
    [ApiController]
    [Route("api/fiscal-configuration")]
    public class FiscalConfigurationController : ControllerBase
    {
        private readonly IFiscalConfigurationService _service;

        public FiscalConfigurationController(IFiscalConfigurationService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FiscalConfiguration model)
        {
            await _service.Create(model);
            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] FiscalConfiguration model)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            // assign fields or simply call Alter with model.Id = id
            model.Id = id;
            await _service.Alter(model);
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var list = await _service.GetAll();
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
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var cfg = await _service.GetActiveAsync();
            if (cfg == null) return NotFound();
            return Ok(cfg);
        }
    }
}