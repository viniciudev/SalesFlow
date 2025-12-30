using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO.BoxDto;
using Model.Moves;
using Repository;
using Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApiCommercial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoxController : ControllerBase
    {
        private readonly IBoxService _caixaService;

        public BoxController(IBoxService caixaService)
        {
            _caixaService = caixaService;
        }
        [HttpPost("abrir")]
        public async Task<ActionResult> AbrirCaixa([FromHeader]int tenantid, [FromBody] OpenBoxDto dto)
        {
            //var usuarioId = User.GetUserId(); // Método de extensão para pegar userId do token
            dto.IdCompany=tenantid;
            var caixa = await _caixaService.AbrirCaixaAsync( dto);
            return Ok(caixa);
        }

        [HttpPost("fechar/{caixaId}")]
        public async Task<ActionResult> FecharCaixa(int caixaId, [FromBody] CloseBoxDto dto)
        {
            var caixa = await _caixaService.FecharCaixaAsync(caixaId, dto);
            return Ok(caixa);
        }

        [HttpGet("status")]
        public async Task<ActionResult> GetBoxStatus([FromHeader] int tenantid)
        {
            var status = await _caixaService.GetStatusByCompany(tenantid);
            return Ok(status);
        }

        [HttpGet("paged")]
        public async Task<ActionResult> GetMovimentacoes([FromHeader] int tenantid, [FromQuery] Filters filters)
        {
            filters.IdCompany = tenantid;
            PagedResult<Box> movimentacoes = await _caixaService.GetMovimentacoesAsync(filters);
            return Ok(movimentacoes);
        }
    }
}
