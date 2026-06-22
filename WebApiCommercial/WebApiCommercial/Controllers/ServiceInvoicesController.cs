using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO;
using Model.Enums;
using Service;
using Service.Exceptions;
using System;
using System.Threading.Tasks;

namespace WebApiCommercial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServiceInvoicesController : ControllerBase
    {
        private readonly IServiceInvoiceService _service;

        public ServiceInvoicesController(IServiceInvoiceService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lista paginada de NFSe
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] Filters filters, [FromHeader] int tenantid)
        {
            filters.IdCompany = tenantid;
            var data = await _service.GetAllPaged(filters);
            return Ok(data);
        }

        /// <summary>
        /// Obtém detalhes de uma NFSe
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServiceInvoiceResponse>> GetById(int id)
        {
            try
            {
                var data = await _service.GetByIdAsync(id);
                return Ok(data);
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Cria uma nova NFSe a partir de uma ordem de serviço
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ResponseGeneric>> Post([FromBody] ServiceInvoiceCreateRequest model, [FromHeader] int tenantid)
        {
            try
            {
                model.TenantId = tenantid;
                var userId = GetUserId();
                var result = await _service.CreateAsync(model, userId);
                return Ok(new ResponseGeneric { Success = true, Message = "NFSe criada com sucesso.", Data = result });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza uma NFSe pendente
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ResponseGeneric>> Put(int id, [FromBody] ServiceInvoiceUpdateRequest model)
        {
            try
            {
                var result = await _service.UpdateAsync(id, model);
                return Ok(new ResponseGeneric { Success = true, Message = "NFSe atualizada com sucesso.", Data = result });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Emite uma NFSe (muda status de Pendente para Emitido)
        /// </summary>
        [HttpPatch("{id:int}/emitir")]
        public async Task<ActionResult<ResponseGeneric>> Emitir(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _service.EmitirAsync(id, userId);
                return Ok(new ResponseGeneric { Success = true, Message = "NFSe emitida com sucesso.", Data = result });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Cancela uma NFSe
        /// </summary>
        [HttpPatch("{id:int}/cancelar")]
        public async Task<ActionResult<ResponseGeneric>> Cancelar(int id, [FromBody] ServiceInvoiceCancelRequest model)
        {
            try
            {
                var userId = GetUserId();
                var result = await _service.CancelarAsync(id, model.CancelReason, userId);
                return Ok(new ResponseGeneric { Success = true, Message = "NFSe cancelada com sucesso.", Data = result });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Reenvia uma NFSe com dados atualizados
        /// </summary>
        [HttpPost("{id:int}/resend")]
        public async Task<ActionResult<ResponseGeneric>> Resend(int id)
        {
            try
            {
                var result = await _service.ResendAsync(id);
                return Ok(new ResponseGeneric { Success = true, Message = "NFSe reenviada com sucesso.", Data = result });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém o próximo número DPS
        /// </summary>
        [HttpGet("next-number")]
        public async Task<ActionResult> GetNextNumber([FromHeader] int tenantid)
        {
            var number = await _service.GetNextNumber(tenantid);
            return Ok(new ResponseGeneric { Success = true, Data = number });
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;
            return Guid.Empty;
        }
    }
}
