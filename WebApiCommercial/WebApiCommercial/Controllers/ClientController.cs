using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO;
using Repository;
using Service;
using System;
using System.Threading.Tasks;

namespace WebAppCommercial.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientController : ControllerBase
    {
        private readonly IClientService clientService;

        public ClientController(IClientService clientService)
        {
            this.clientService = clientService;

        }
        [HttpGet]
        public async Task<ActionResult<PagedResult<Client>>> Get([FromQuery] Filters filter,
          [FromHeader] int tenantid)
        {
            filter.IdCompany = tenantid;
            var pagedData = await clientService.GetAllPaged(filter);

            return Ok(pagedData);
        }

        [HttpGet("filters")]
        public async Task<ActionResult> GetByFilter([FromQuery] Filters filter,
                                                    [FromHeader] int tenantid)
        {
            filter.IdCompany = tenantid;
            var client = await clientService.GetByFilter(filter);

            return Ok(client);
        }

        // GET api/<ClientController>/5
        [HttpGet("GetByMonthAllClients")]
        public async Task<ActionResult<ClientInfoResponse>> GetByMonthAllClients([FromQuery] Filters filter,
      [FromHeader] int tenantid)
        {
            filter.IdCompany = tenantid;
            return Ok(await clientService.GetByMonthAllClients(filter));
        }

    
        /// <summary>
        /// Carrega um parceiro pelo Id (tela de edicao).
        /// A restricao ":int" evita ambiguidade com as rotas literais deste
        /// controller ("filters", "exists", "GetByMonthAllClients").
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Client>> GetById(int id, [FromHeader] int tenantid)
        {
            var client = await clientService.GetById(id, tenantid);

            if (client == null)
            {
                return NotFound(new { message = "Cliente nao encontrado." });
            }

            return Ok(client);
        }

        [HttpPost]
        public async Task<ActionResult<dynamic>> Post([FromBody] ClientDto model, [FromHeader] int tenantid)
        {
            model.IdCompany = tenantid;
            model.CreatDate = DateTime.Now;

            // RN01/RN11 — não duplicar pessoa na mesma empresa. O índice único
            // em tb_client é a garantia real; esta checagem existe para devolver
            // uma mensagem tratada em vez de um erro de constraint do Postgres.
            if (await clientService.DocumentExists(tenantid, model.Document))
            {
                return Conflict(new
                {
                    message = "Ja existe um parceiro cadastrado com este CPF/CNPJ nesta empresa."
                });
            }

            await clientService.SaveClient(model);
            return Ok(model);
        }


        // PUT api/<ClientController>/5
        [HttpPut()]
        public async Task<ActionResult<dynamic>> Put([FromBody] Client model)
        {
            if (await clientService.DocumentExists(model.IdCompany, model.Document, model.Id))
            {
                return Conflict(new
                {
                    message = "Ja existe um parceiro cadastrado com este CPF/CNPJ nesta empresa."
                });
            }

            try
            {
                await clientService.Alter(model);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
  

            return true;

        }

        /// <summary>
        /// RN01/RN11 — consulta de existência de CPF/CNPJ por empresa.
        /// Usado pelo front para validar duplicidade antes de salvar.
        /// </summary>
        [HttpGet("exists")]
        public async Task<ActionResult<dynamic>> Exists(
            [FromQuery] string document,
            [FromQuery] int? ignoreId,
            [FromHeader] int tenantid)
        {
            var exists = await clientService.DocumentExists(tenantid, document, ignoreId);
            return Ok(new { exists });
        }

        // DELETE api/<ClientController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {

        }
    }
}
