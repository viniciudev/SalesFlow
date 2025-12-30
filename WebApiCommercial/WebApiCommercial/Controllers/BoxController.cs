using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.DTO.BoxDto;
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
        public async Task<ActionResult> VerificarStatus()
        {
            //var status = await _caixaService.VerificarStatusCaixaAsync();
            //return Ok(status);
            return Ok();
        }

        //[HttpGet("movimentacoes/{caixaId}")]
        //public async Task<ActionResult<List<MovimentacaoDTO>>> GetMovimentacoes(Guid caixaId)
        //{
        //    var movimentacoes = await _caixaService.GetMovimentacoesAsync(caixaId);
        //    return Ok(movimentacoes);
        //}
    }
}
