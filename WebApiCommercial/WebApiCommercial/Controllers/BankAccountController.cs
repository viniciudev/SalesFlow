using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Repository;
using Service;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApiCommercial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountService _bankAccountService;

        public BankAccountController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        // GET: api/BankAccount
        [HttpGet]
        public async Task<ActionResult<PagedResult<BankAccountDto>>> GetPaged([FromHeader]int tenantid, [FromQuery] BankAccountFilterDto filter)
        {
            filter.IdCompany = tenantid;
            var result = await _bankAccountService.GetPagedAsync(filter);
            return Ok(result);
        }

        // GET: api/BankAccount/filters?textOption=search
        [HttpGet("filters")]
        public async Task<ActionResult<IEnumerable<BankAccountDto>>> GetByFilter([FromHeader]int tenantid, [FromQuery] string textOption)
        {
            var result = await _bankAccountService.GetByFilterAsync(tenantid, textOption);
            return Ok(result);
        }

        // GET: api/BankAccount/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BankAccountDto>> GetById(int id)
        {
            var result = await _bankAccountService.GetByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // POST: api/BankAccount
        [HttpPost]
        public async Task<ActionResult<BankAccountDto>> Create([FromHeader]int tenantid, [FromBody] CreateBankAccountDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            dto.idCompany = tenantid;
            var result = await _bankAccountService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // PUT: api/BankAccount
        [HttpPut]
        public async Task<ActionResult<BankAccountDto>> Update([FromBody] UpdateBankAccountDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _bankAccountService.ExistsAsync(dto.Id);
            if (!exists)
                return NotFound();

            var result = await _bankAccountService.UpdateAsync(dto);
            return Ok(result);
        }

        // DELETE: api/BankAccount/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _bankAccountService.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // PATCH: api/BankAccount/5/toggle-status
        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<BankAccountDto>> ToggleStatus(int id)
        {
            var result = await _bankAccountService.ToggleStatusAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}