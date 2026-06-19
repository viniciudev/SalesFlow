using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO;
using Model.Registrations;
using Service;
using Service.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApiCommercial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceProvidedController : ControllerBase
    {
        private readonly IServiceProvidedService serviceProvidedService;

        public ServiceProvidedController(IServiceProvidedService serviceProvidedService)
        {
            this.serviceProvidedService = serviceProvidedService;
        }

        /// <summary>
        /// Lista paginada de serviços prestados com suporte a filtros
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] Filters filters, [FromHeader] int tenantid)
        {
            filters.IdCompany = tenantid;
            var data = await serviceProvidedService.GetAllPaged(filters);
            return Ok(data);
        }

        /// <summary>
        /// Lista serviços por nome (para autocomplete)
        /// </summary>
        [HttpGet("GetListByName")]
        public async Task<ActionResult<List<ServiceProvided>>> GetListByName([FromQuery] Filters filter, [FromHeader] int tenantid)
        {
            filter.IdCompany = tenantid;
            var data = await serviceProvidedService.GetListByName(filter);
            return Ok(data);
        }

        /// <summary>
        /// Obtém um serviço prestado por ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServiceProvidedResponse>> GetById(int id)
        {
            var data = await serviceProvidedService.GetByIdDtoAsync(id);
            if (data == null)
                return NotFound(new ResponseGeneric { Success = false, Message = "Serviço prestado não encontrado." });
            return Ok(data);
        }

        /// <summary>
        /// Cria um novo serviço prestado (NFS-e 2026)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ResponseGeneric>> Post([FromBody] ServiceProvidedCreateRequest model, [FromHeader] int tenantid)
        {
            try
            {
                model.IdCompany = tenantid;
                var result = await serviceProvidedService.CreateAsync(model);
                return Ok(new ResponseGeneric { Success = true, Message = "Serviço criado com sucesso.", Data = result });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza um serviço prestado existente (NFS-e 2026)
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ResponseGeneric>> Put(int id, [FromBody] ServiceProvidedUpdateRequest model)
        {
            try
            {
                var result = await serviceProvidedService.UpdateAsync(id, model);
                return Ok(new ResponseGeneric { Success = true, Message = "Serviço atualizado com sucesso.", Data = result });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Exclui um serviço prestado
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ResponseGeneric>> Delete(int id)
        {
            try
            {
                await serviceProvidedService.DeleteAsync(id);
                return Ok(new ResponseGeneric { Success = true, Message = "Serviço excluído com sucesso." });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }
    }
}
