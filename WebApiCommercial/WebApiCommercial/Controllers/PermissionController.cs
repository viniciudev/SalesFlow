using Microsoft.AspNetCore.Mvc;
using Service;

namespace WebApiCommercial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }
        //[HttpPost("abrir")]
        //public async Task<ActionResult> AbrirCaixa([FromHeader]int tenantid, [FromBody] OpenPermissionDto dto)
        //{
        //    //var usuarioId = User.GetUserId(); // Método de extensão para pegar userId do token
        //    dto.IdCompany=tenantid;
        //    var caixa = await _caixaService.AbrirCaixaAsync( dto);
        //    return Ok(caixa);
        //}

        //[HttpPost("fechar/{caixaId}")]
        //public async Task<ActionResult> FecharCaixa(int caixaId, [FromBody] ClosePermissionDto dto)
        //{
        //    var caixa = await _caixaService.FecharCaixaAsync(caixaId, dto);
        //    return Ok(caixa);
        //}

        //[HttpGet("status")]
        //public async Task<ActionResult> GetPermissionStatus([FromHeader] int tenantid)
        //{
        //    var status = await _caixaService.GetStatusByCompany(tenantid);
        //    return Ok(status);
        //}

        //[HttpGet("paged")]
        //public async Task<ActionResult> GetMovimentacoes([FromHeader] int tenantid, [FromQuery] Filters filters)
        //{
        //    filters.IdCompany = tenantid;
        //    PagedResult<Permission> movimentacoes = await _caixaService.GetMovimentacoesAsync(filters);
        //    return Ok(movimentacoes);
        //}
    }
}
