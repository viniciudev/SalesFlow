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
    public class ServiceOrdersController : ControllerBase
    {
        private readonly IServiceOrderService _service;

        public ServiceOrdersController(IServiceOrderService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lista paginada de ordens de serviço
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] Filters filters, [FromHeader] int tenantid)
        {
            filters.IdCompany = tenantid;
            var data = await _service.GetAllPaged(filters);
            return Ok(data);
        }

        /// <summary>
        /// Obtém detalhes de uma ordem de serviço
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServiceOrderResponse>> GetById(int id)
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
        /// Cria uma nova ordem de serviço
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ResponseGeneric>> Post([FromBody] ServiceOrderCreateRequest model, [FromHeader] int tenantid)
        {
            try
            {
                model.TenantId = tenantid;
                var userId = GetUserId();
                var result = await _service.CreateAsync(model, userId);
                return Ok(new ResponseGeneric { Success = true, Message = "Ordem de serviço criada com sucesso.", Data = result });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza uma ordem de serviço
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ResponseGeneric>> Put(int id, [FromBody] ServiceOrderUpdateRequest model)
        {
            try
            {
                var userId = GetUserId();
                var result = await _service.UpdateAsync(id, model, userId);
                return Ok(new ResponseGeneric { Success = true, Message = "Ordem de serviço atualizada com sucesso.", Data = result });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Altera o status da ordem de serviço
        /// </summary>
        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult<ResponseGeneric>> PatchStatus(int id, [FromBody] ServiceOrderStatusRequest model)
        {
            try
            {
                var userId = GetUserId();
                var result = await _service.ChangeStatusAsync(id, model.Status, userId);
                return Ok(new ResponseGeneric { Success = true, Message = "Status alterado com sucesso.", Data = result });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Cancela uma ordem de serviço
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ResponseGeneric>> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new ResponseGeneric { Success = true, Message = "Ordem de serviço cancelada com sucesso." });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Lista NFSe da ordem de serviço
        /// </summary>
        [HttpGet("{id:int}/invoices")]
        public async Task<ActionResult> GetInvoices(int id, [FromHeader] int tenantid)
        {
            try
            {
                var data = await _service.GetByIdAsync(id);
                return Ok(new ResponseGeneric { Success = true, Data = data.Invoices });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Serviços disponíveis para NFSe
        /// </summary>
        [HttpGet("{id:int}/available-services")]
        public async Task<ActionResult> GetAvailableServices(int id)
        {
            try
            {
                var data = await _service.GetAvailableServices(id);
                return Ok(new ResponseGeneric { Success = true, Data = data });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
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
