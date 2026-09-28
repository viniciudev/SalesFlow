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
    /// Manifesto Eletrônico de Documentos Fiscais (modelo 58) — <b>fase 1</b>:
    /// montar, assinar e validar o XML contra os XSDs da SEFAZ.
    ///
    /// <b>Não há transmissão aqui.</b> A rota <c>transmitir</c> e a
    /// <c>danfe</c> ficam para a fase 2, junto de encerramento e cancelamento.
    /// O que esta API entrega é o XML assinado e validado — e por isso ela é
    /// verificável por completo neste ambiente, sem certificado A1 real e sem
    /// rede.
    ///
    /// <b>Rota:</b> <c>[controller]</c> resolve para <c>Mdfe</c>, ou seja
    /// <c>/api/Mdfe</c> — PascalCase, como <c>/api/Vehicle</c> e
    /// <c>/api/Client</c>. O roteamento do ASP.NET é insensível a caixa, então
    /// a rota em minúsculas do pedido original (<c>/api/mdfe/emitir</c>)
    /// continua funcionando; o que mudou foi só o nome da ação, que virou
    /// <c>POST /api/Mdfe</c> como nos demais controllers.
    ///
    /// <b>Sobre o <c>:int</c> nas rotas com <c>{id}</c>:</b> não é decorativo —
    /// é o que desambigua <c>GET /{id:int}</c> de <c>GET /pesquisar-documentos</c>
    /// e <c>GET /proximo-numero</c>. Sem o constraint, as duas rotas literais
    /// casariam com <c>{id}</c> e a requisição morreria em erro de conversão de
    /// inteiro.
    ///
    /// <b>Permissão:</b> o <c>ConventionPermissionMiddleware</c> bloqueia por
    /// padrão todo controller fora do mapa. Este depende de
    /// <c>{"Mdfe", "FISCAL_MDFE"}</c> registrado em <b>dois</b> lugares —
    /// <c>ConventionPermissionMiddleware._controllerPermissionMap</c> E
    /// <c>Startup.ConfigurePermissionMappings()</c>, que sobrescreve o mapa
    /// estático no boot. Faltando qualquer um dos dois, TODA rota daqui
    /// responde 403, e o 403 é indistinguível de uma negação legítima.
    ///
    /// <b>Erro de regra devolve HTTP 200</b> com <c>Success = false</c>, como em
    /// <c>VehicleController</c> — o front já sabe ler esse envelope, e trocar
    /// para 4xx quebraria o tratamento de erro das outras telas.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MdfeController : ControllerBase
    {
        private readonly IMdfeService _service;

        public MdfeController(IMdfeService service)
        {
            _service = service;
        }

        /// <summary>
        /// Listagem paginada dos manifestos (filtros: situação, tipo de
        /// emitente, tipo de operação, UFs e chave/número).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] Filters filters, [FromHeader] int tenantid)
        {
            // A empresa vem SEMPRE do header, nunca da query: aceitar IdCompany
            // pelo filtro deixaria qualquer usuário listar os manifestos de
            // outra empresa trocando um parâmetro. O isolamento neste projeto é
            // manual em cada método do repositório, então este é o ponto onde o
            // IdCompany legítimo entra.
            filters.IdCompany = tenantid;

            var data = await _service.GetPagedAsync(filters);
            return Ok(data);
        }

        /// <summary>
        /// Detalhe do manifesto — já com <c>Validacao</c> resolvida, para a aba
        /// Resumo não precisar de uma segunda chamada só para pintar o
        /// checklist.
        /// </summary>
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

        /// <summary>
        /// Série e próximo número para o cabeçalho da tela de emissão.
        ///
        /// <b>Não reserva número</b> — devolve uma previsão. Reservar aqui
        /// queimaria um número a cada vez que o usuário abrisse a tela e
        /// desistisse, e a numeração do MDF-e não aceita buracos. A reserva de
        /// verdade acontece dentro da transação que grava o manifesto.
        /// </summary>
        [HttpGet("proximo-numero")]
        public async Task<ActionResult<ResponseGeneric>> GetProximoNumero([FromHeader] int tenantid)
        {
            try
            {
                var data = await _service.GetProximoNumeroAsync(tenantid);
                return Ok(new ResponseGeneric { Success = true, Data = data });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Documentos fiscais manifestáveis — o endpoint da área vermelha da
        /// tela. Unifica NF-e de saída (própria) e de entrada (compra de
        /// terceiros), com a elegibilidade já resolvida.
        ///
        /// Documento inelegível <b>continua na lista</b>, com o motivo em
        /// <c>Impedimentos</c>: esconder faria o usuário procurar uma nota que
        /// existe e não entender o silêncio.
        /// </summary>
        [HttpGet("pesquisar-documentos")]
        public async Task<ActionResult<ResponseGeneric>> PesquisarDocumentos([FromQuery] Filters filters, [FromHeader] int tenantid)
        {
            var data = await _service.PesquisarDocumentosAsync(filters, tenantid);
            return Ok(new ResponseGeneric { Success = true, Data = data });
        }

        /// <summary>
        /// Cria o rascunho do manifesto, com documentos, percurso, veículos e
        /// condutores. O número é atribuído pelo servidor, dentro da transação
        /// que grava — não vem no corpo.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ResponseGeneric>> Post([FromBody] MdfeCreateDto model, [FromHeader] int tenantid)
        {
            try
            {
                var result = await _service.CriarAsync(model, tenantid);
                return Ok(new ResponseGeneric
                {
                    Success = true,
                    Message = "Manifesto criado com sucesso.",
                    Data = result
                });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Edita o rascunho. Um manifesto que já teve o XML gerado não é mais
        /// editável: alterar a origem invalidaria a assinatura, e o manifesto
        /// declarado não volta a ser rascunho. O caminho é excluir e emitir
        /// outro (o cancelamento é fase 2).
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ResponseGeneric>> Put(int id, [FromBody] MdfeUpdateDto model, [FromHeader] int tenantid)
        {
            try
            {
                var result = await _service.AtualizarAsync(id, model, tenantid);
                return Ok(new ResponseGeneric
                {
                    Success = true,
                    Message = "Manifesto atualizado com sucesso.",
                    Data = result
                });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>Exclui um rascunho. Manifesto com XML gerado é recusado.</summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ResponseGeneric>> Delete(int id, [FromHeader] int tenantid)
        {
            try
            {
                await _service.DeletarAsync(id, tenantid);
                return Ok(new ResponseGeneric { Success = true, Message = "Manifesto excluído com sucesso." });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Checklist RM01–RM14 sem gerar o XML — é o botão "Validar" da aba
        /// Resumo. Devolve TODAS as pendências de uma vez, não só a primeira.
        /// </summary>
        [HttpGet("{id:int}/validacao")]
        public async Task<ActionResult<ResponseGeneric>> GetValidacao(int id, [FromHeader] int tenantid)
        {
            try
            {
                var data = await _service.ValidarAsync(id, tenantid);
                return Ok(new ResponseGeneric { Success = true, Data = data });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Monta, <b>assina</b> e valida o XML contra os XSDs da SEFAZ, e
        /// persiste. É o "Assinar" do mockup — assinar não é um passo separado
        /// porque um XML assinado mas não validado não serve para nada, e o
        /// validador precisa da assinatura para conferir o schema.
        ///
        /// Falha de schema devolve <c>Success = false</c> e o rascunho é
        /// preservado, com o motivo em <c>ErrorMessage</c>: perder o manifesto
        /// digitado por causa de um campo fora do leiaute seria pior que o erro.
        /// </summary>
        [HttpPost("{id:int}/gerar-xml")]
        public async Task<ActionResult<ResponseGeneric>> GerarXml(int id, [FromHeader] int tenantid)
        {
            try
            {
                var data = await _service.GerarXmlAsync(id, tenantid);
                return Ok(new ResponseGeneric
                {
                    Success = true,
                    Message = "XML gerado, assinado e validado com sucesso.",
                    Data = data
                });
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Download do XML assinado.
        ///
        /// Devolve o arquivo cru, e não o envelope <c>ResponseGeneric</c>: é
        /// download, e o front usa <c>blob</c>. Um erro de regra, porém,
        /// continua saindo como JSON com <c>Success = false</c> — o axios do
        /// front confere o content-type antes de baixar, para não salvar um
        /// JSON de erro com nome de <c>.xml</c>.
        /// </summary>
        [HttpGet("{id:int}/xml")]
        public async Task<ActionResult> GetXml(int id, [FromHeader] int tenantid)
        {
            try
            {
                var (bytes, nomeArquivo) = await _service.ObterXmlAsync(id, tenantid);
                return File(bytes, "application/xml", nomeArquivo);
            }
            catch (DomainException ex)
            {
                return Ok(new ResponseGeneric { Success = false, Message = ex.Message });
            }
        }
    }
}
