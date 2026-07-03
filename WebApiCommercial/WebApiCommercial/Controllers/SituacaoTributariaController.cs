using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO;
using Model.Registrations;
using Repository;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiCommercial.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SituacaoTributariaController : ControllerBase
    {
        private readonly ContextBase _db;

        public SituacaoTributariaController(ContextBase db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromHeader] int tenantid)
        {
            var list = await _db.Set<SituacaoTributaria>()
                .Where(x => x.CompanyId == tenantid)
                .Select(x => new
                {
                    x.Id,
                    x.Codigo,
                    x.Descricao
                })
                .AsNoTracking()
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromHeader] int tenantid, [FromBody] SituacaoTributaria model)
        {
            model.CompanyId = tenantid;
            model.Id = 0;
            _db.Set<SituacaoTributaria>().Add(model);
            await _db.SaveChangesAsync();
            return Ok(new ResponseGeneric { Success = true, Data = new { model.Id, model.Codigo, model.Descricao } });
        }
    }
}
