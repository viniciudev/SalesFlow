using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO;
using Model.DTO.NFe;
using Model.Enums;
using Model.Registrations;
using Repository;
using Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApiCommercial.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NFeController : ControllerBase
    {
        private readonly INFeService _nfeService;

        public NFeController(INFeService nfeService)
        {
            _nfeService = nfeService;
        }

        // GET api/nfe/pending
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var list = await _nfeService.GetPendingAsync();
            return Ok(list);
        }

        // GET api/nfe
        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader]int tenantid, [FromQuery] Filters filters)
        {
            filters.IdCompany = tenantid;
            PagedResult<NFeEmission> list = await _nfeService.GetPaged(filters);
            return Ok(list);
        }

        // GET api/nfe/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _nfeService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // POST api/nfe
        // Cria tentativa de emissão (salva payload para retry)
        [HttpPost]
        public async Task<IActionResult> CreateAttempt([FromHeader]int tenantid,[FromBody] NFeEmissionDto attempt)
        {
            if (attempt == null) return BadRequest("Payload inválido.");
            try
            {
                attempt.CompanyId = tenantid;
                var resp = await _nfeService.CreateAttemptAsync(attempt);
                return Ok(resp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpPut]
        public async Task<IActionResult> Update( [FromBody] NFeEmissionDto attempt)
        {
            if (attempt == null) return BadRequest("Payload inválido.");
            try
            {
               
                await _nfeService.update(attempt);
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        // POST api/nfe
        // reenvio de emissão (salva payload para retry)
        [HttpPost("{id}/resend")]
        public async Task<IActionResult> Resend(int id)
        {
            try
            {
                var resp = await _nfeService.Resend(id);
                return Ok(resp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet("{id}/danfe")]
        public async Task<IActionResult> Danfe(int id)
        {
            try
            {
                byte[] resp = await _nfeService.Danfe(id);
                return Ok(resp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet("{id}/xml")]
        public async Task<IActionResult> BaixarXml(int id)
        {
            try
            {
                var xmlBytes = await _nfeService.ObterXml(id);
                var fileName = await _nfeService.ObterNomeArquivoXml(id);

                return File(xmlBytes, "application/xml", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpPost("Cancelar")]
        public async Task<IActionResult> CancelarNfe( [FromBody] CancelarNotaRequest cancelarNota)
        {
            try
            {
                var result = await _nfeService.CancelarNfe(cancelarNota);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT api/nfe/{id}/result
        // Atualiza resultado da emissão (success/failure, número, response)
        [HttpPut("{id:int}/result")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] NFeResultRequest req)
        {
            if (req == null) return BadRequest("Payload inválido.");
            try
            {
                await _nfeService.UpdateResultAsync(id, req.Sent, req.Numero, req.ResponseJson, req.ErrorMessage);
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST api/nfe/{id}/resend
        // Retorna o payload salvo para reenvio e incrementa contador de tentativas
        //[HttpPost("{id:int}/resend")]
        //public async Task<IActionResult> Resend(int id)
        //{
        //    var existing = await _nfeService.GetByIdAsync(id);
        //    if (existing == null) return NotFound();

        //    try
        //    {
        //        // incrementa TryCount utilizando UpdateResultAsync mantendo os dados atuais
        //        await _nfeService.UpdateResultAsync(existing.Id, existing.Sent, existing.Numero, existing.ResponseJson, existing.ErrorMessage);

        //        // Retorna o payload salvo para o processo que fará o reenvio
        //        return Ok(new
        //        {
        //            id = existing.Id,
        //            tipoDocumento = existing.TipoDocumento,
        //            serie = existing.Serie,
        //            payload = existing.RequestPayloadJson
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { error = ex.Message });
        //    }
        //}

        // GET api/nfe/last-number?serie=XXX&tipo=NFE
        [HttpGet("last-number")]
        public async Task<IActionResult> GetLastNumber([FromQuery] string serie, [FromQuery] string tipo)
        {
            if (string.IsNullOrWhiteSpace(serie) || string.IsNullOrWhiteSpace(tipo))
                return BadRequest("Parâmetros 'serie' e 'tipo' são obrigatórios.");

            if (!Enum.TryParse<TipoDocumentoEnum>(tipo, true, out var tipoDoc))
                return BadRequest("Tipo de documento inválido. Use NFE ou NFCE.");

            var last = await _nfeService.GetLastNumeroAsync(serie, tipoDoc);
            return Ok(new { serie, tipo = tipoDoc.ToString(), lastNumero = last });
        }
    }

    public class NFeResultRequest
    {
        public bool Sent { get; set; }
        public long? Numero { get; set; }
        public string? ResponseJson { get; set; }
        public string? ErrorMessage { get; set; }
    }
}