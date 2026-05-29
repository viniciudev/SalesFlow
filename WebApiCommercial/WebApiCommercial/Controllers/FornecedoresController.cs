using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Registrations;
using Repository;
using Service;
using System.Threading.Tasks;

namespace WebApiCommercial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FornecedoresController : ControllerBase
    {
        private readonly IProviderService providerService;

        public FornecedoresController(IProviderService providerService)
        {
            this.providerService = providerService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<Provider>>> GetPaged([FromQuery] Filters filters,
            [FromHeader] int tenantid)
        {
            filters.IdCompany = tenantid;
            return Ok(await providerService.GetAllPaged(filters));
        }

        [HttpGet("GetListByName")]
        public async Task<ActionResult> GetListByName([FromQuery] Filters filters,
            [FromHeader] int tenantid)
        {
            filters.IdCompany = tenantid;
            return Ok(await providerService.GetListByName(filters));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Provider>> GetById(int id)
        {
            return Ok(await providerService.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<ActionResult<int>> Post([FromBody] Provider provider,
            [FromHeader] int tenantid)
        {
            provider.IdCompany = tenantid;
            await providerService.Save(provider);
            return Ok(provider.Id);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] Provider provider,
            [FromHeader] int tenantid)
        {
            provider.Id = id;
            provider.IdCompany = tenantid;
            await providerService.Alter(provider);
            return Ok(true);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await providerService.DeleteAsync(id);
            return Ok(true);
        }
    }
}
