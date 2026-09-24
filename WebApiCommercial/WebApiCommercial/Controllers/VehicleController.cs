using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO;
using Service;
using Service.Exceptions;
using System.Threading.Tasks;

namespace WebApiCommercial.Controllers
{
    /// <summary>
    /// Cadastro de veículos para emissão de MDF-e (OS-002).
    ///
    /// Sobre a rota: <c>[controller]</c> resolve para <c>Vehicle</c>, ou seja
    /// <c>/api/Vehicle</c> — a OS pedia <c>/api/vehicles</c>, mas o projeto usa
    /// PascalCase nos controllers (<c>/api/Client</c>, <c>/api/Product</c>) e o
    /// roteamento do ASP.NET é insensível a caixa, então quem chamar em
    /// minúsculas continua funcionando.
    ///
    /// Sobre o <c>:int</c> nas rotas com <c>{id}</c>: ele não é decorativo —
    /// é o que desambigua <c>GET /{id}</c> de <c>GET /available-for-mdfe</c>.
    /// Sem o constraint, "available-for-mdfe" casaria com <c>{id}</c> e a
    /// requisição morreria em erro de conversão de inteiro.
    ///
    /// <b>Permissão:</b> o <c>ConventionPermissionMiddleware</c> bloqueia por
    /// padrão todo controller fora do mapa de permissões. Este controller
    /// depende de <c>{"Vehicle", "CADASTRO_VEICULO"}</c> registrado em
    /// <c>ConventionPermissionMiddleware</c> e em <c>Startup.ConfigurePermissionMappings</c>
    /// — sem isso, TODA rota daqui responde 403, e o 403 é indistinguível de
    /// uma negação legítima de permissão.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _service;

        public VehicleController(IVehicleService service)
        {
            _service = service;
        }

        /// <summary>
        /// Listagem paginada com filtros (placa/código interno, UF, tipo,
        /// ativo). Devolve ativos E inativos.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] Filters filters, [FromHeader] int tenantid)
        {
            // A empresa vem SEMPRE do header, nunca da query: aceitar IdCompany
            // pelo filtro deixaria qualquer usuário listar a frota de outra
            // empresa trocando um parâmetro.
            filters.IdCompany = tenantid;

            var data = await _service.GetPagedAsync(filters);
            return Ok(data);
        }

        /// <summary>Detalhe do veículo, com o checklist de MDF-e já resolvido.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ResponseGeneric>> GetById(int id, [FromHeader] int tenantid)
        {
            try
            {
                var data = await _service.GetByIdAsync(id, tenantid);
                return Ok(new ResponseGeneric { Success = true, Data = data });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>Cadastra um veículo.</summary>
        [HttpPost]
        public async Task<ActionResult<ResponseGeneric>> Post([FromBody] VehicleCreateDto model, [FromHeader] int tenantid)
        {
            try
            {
                var result = await _service.CreateAsync(model, tenantid);
                return Ok(new ResponseGeneric
                {
                    Success = true,
                    Message = "Veículo cadastrado com sucesso.",
                    Data = result
                });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza um veículo. <c>IsActive</c> vem no corpo (RV11): o mesmo
        /// endpoint edita e ativa/desativa — os PATCH abaixo existem para a
        /// ação isolada na listagem.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ResponseGeneric>> Put(int id, [FromBody] VehicleUpdateDto model, [FromHeader] int tenantid)
        {
            try
            {
                var result = await _service.UpdateAsync(id, model, tenantid);
                return Ok(new ResponseGeneric
                {
                    Success = true,
                    Message = "Veículo atualizado com sucesso.",
                    Data = result
                });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// RV11 — exclusão lógica. Não existe DELETE físico de veículo: o
        /// registro sai da seleção de MDF-e (RV12) e continua na listagem.
        /// </summary>
        [HttpPatch("{id:int}/deactivate")]
        public async Task<ActionResult<ResponseGeneric>> Deactivate(int id, [FromHeader] int tenantid)
        {
            try
            {
                await _service.DeactivateAsync(id, tenantid);
                return Ok(new ResponseGeneric { Success = true, Message = "Veículo desativado com sucesso." });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>Reativa um veículo desativado (RV11).</summary>
        [HttpPatch("{id:int}/activate")]
        public async Task<ActionResult<ResponseGeneric>> Activate(int id, [FromHeader] int tenantid)
        {
            try
            {
                await _service.ActivateAsync(id, tenantid);
                return Ok(new ResponseGeneric { Success = true, Message = "Veículo reativado com sucesso." });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Checklist "Apto para MDF-e": placa, UF, rodado, carroceria, tara e
        /// (se terceiro) dados do proprietário.
        /// </summary>
        [HttpGet("{id:int}/mdfe-status")]
        public async Task<ActionResult<ResponseGeneric>> GetMdfEStatus(int id, [FromHeader] int tenantid)
        {
            try
            {
                var status = await _service.GetMdfEStatusAsync(id, tenantid);
                return Ok(new ResponseGeneric { Success = true, Data = status });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// RV12 — somente veículos ATIVOS, para os seletores de MDF-e.
        ///
        /// Devolve a lista com o checklist de cada um: o front-end decide se
        /// desabilita (convenção do <c>driver.ts</c>, que mantém o item visível
        /// para o usuário entender o motivo) ou se filtra os inaptos.
        /// </summary>
        [HttpGet("available-for-mdfe")]
        public async Task<ActionResult<ResponseGeneric>> GetAvailableForMdfE([FromHeader] int tenantid)
        {
            var data = await _service.GetAvailableForMdfEAsync(tenantid);
            return Ok(new ResponseGeneric { Success = true, Data = data });
        }
    }
}
