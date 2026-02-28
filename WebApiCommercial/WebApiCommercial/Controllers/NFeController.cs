using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Model.Enums;
using Model.Registrations;
using Service;

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
        public async Task<IActionResult> GetAll([FromHeader]int tenantid)
        {
            List<NFeEmission> list = await _nfeService.GetAll(tenantid);
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
        public async Task<IActionResult> CreateAttempt([FromBody] NFeEmission attempt)
        {
            if (attempt == null) return BadRequest("Payload inválido.");
            try
            {
                var id = await _nfeService.CreateAttemptAsync(attempt);
                return CreatedAtAction(nameof(GetById), new { id = id }, new { id = id });
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
        [HttpPost("{id:int}/resend")]
        public async Task<IActionResult> Resend(int id)
        {
            var existing = await _nfeService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            try
            {
                // incrementa TryCount utilizando UpdateResultAsync mantendo os dados atuais
                await _nfeService.UpdateResultAsync(existing.Id, existing.Sent, existing.Numero, existing.ResponseJson, existing.ErrorMessage);

                // Retorna o payload salvo para o processo que fará o reenvio
                return Ok(new
                {
                    id = existing.Id,
                    tipoDocumento = existing.TipoDocumento,
                    serie = existing.Serie,
                    payload = existing.RequestPayloadJson
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

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