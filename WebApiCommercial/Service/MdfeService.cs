#nullable enable
using DFe.Classes.Entidades;
using DFe.Classes.Flags;
using DFe.Utils;
using MDFe.Classes.Extensoes;
using MDFe.Utils.Configuracoes;
using MDFe.Utils.Flags;
using Microsoft.AspNetCore.Hosting;
using Model;
using Model.DTO;
using Model.Enums;
using Model.MDFe;
using Model.Moves;
using Model.Registrations;
using Repository;
using Service.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

// Mesmo apelido que a biblioteca vendorizada usa (ExtMDFe.cs): o nome do tipo é
// MDFe.Classes.Informacoes.MDFe, e escrever isso inteiro em cada chamada de
// FuncoesXml.XmlStringParaClasse<> não ajuda a ler.
using MDFEletronico = MDFe.Classes.Informacoes.MDFe;

namespace Service
{
    /// <summary>
    /// Emissão de MDF-e (modelo 58) — monta e assina o XML, valida contra os XSDs
    /// da SEFAZ, transmite (<c>MDFeRecepcaoSinc</c>) e opera o ciclo de vida do
    /// documento: consulta de situação, encerramento, cancelamento e a DAMDFe.
    ///
    /// <b>O MDF-e é síncrono.</b> Não há lote, recibo nem polling como na NF-e: o
    /// envio já devolve protocolo ou rejeição, e por isso não existe aqui o par
    /// "transmitir / consultar recibo" que o <c>NFeService</c> tem. A consulta de
    /// situação existe para o outro caso — saber o que a SEFAZ pensa de um
    /// manifesto cuja resposta se perdeu no caminho.
    ///
    /// <see cref="MdfeStatus.Validado"/> não é autorização: é "o XML está bem
    /// formado, assinado e passa no schema". Só depois da transmissão o manifesto
    /// vira <see cref="MdfeStatus.Autorizado"/>, e é quando ganha protocolo e
    /// <c>DataAutorizacao</c> — a base da janela de 24h do cancelamento.
    ///
    /// Deliberadamente NÃO repete os três vícios do <c>NFeService</c>:
    /// <list type="number">
    ///   <item>nada de <c>dynamic</c> no retorno — os métodos devolvem DTOs
    ///   tipados;</item>
    ///   <item>nada de estado de requisição em campo de instância (o
    ///   <c>NFeService</c> guarda <c>_currentSale</c>, <c>_nfe</c> e
    ///   <c>_configuracaoApp</c> em campos, o que só funciona enquanto o
    ///   lifetime for o certo) — aqui tudo viaja por parâmetro;</item>
    ///   <item>nada de <c>catch</c> que engole o erro e devolve lista vazia —
    ///   erro de regra vira <see cref="DomainException"/>, e falha de schema vira
    ///   mensagem no retorno, com o manifesto marcado como
    ///   <see cref="MdfeStatus.Erro"/>.</item>
    /// </list>
    ///
    /// Uma diferença de desenho em relação à emissão: falha de <b>rede</b> na
    /// transmissão não rebaixa o manifesto. Ela vira <see cref="DomainException"/>
    /// com o motivo e o manifesto continua <see cref="MdfeStatus.Validado"/> —
    /// rebaixá-lo faria a tela dizer que o documento está ruim quando o que está
    /// fora é a SEFAZ, e obrigaria a gerar XML de novo (consumindo número) para
    /// tentar de novo.
    /// </summary>
    public class MdfeService : BaseService<MdfeEmissao>, IMdfeService
    {
        private readonly IMdfeRepository _mdfeRepository;
        private readonly IFiscalConfigurationRepository _fiscalRepository;
        private readonly IVehicleService _vehicleService;
        private readonly IWebHostEnvironment _environment;

        public MdfeService(
            IGenericRepository<MdfeEmissao> repository,
            IMdfeRepository mdfeRepository,
            IFiscalConfigurationRepository fiscalRepository,
            IVehicleService vehicleService,
            IWebHostEnvironment environment) : base(repository)
        {
            _mdfeRepository = mdfeRepository;
            _fiscalRepository = fiscalRepository;
            _vehicleService = vehicleService;
            _environment = environment;
        }

        // =====================================================================
        // Listagem e detalhe
        // =====================================================================

        public async Task<PagedResult<MdfeListItemDto>> GetPagedAsync(Filters filter)
        {
            var paginado = await _mdfeRepository.GetPagedAsync(filter);

            // Copia os metadados em vez de herdar PagedResult<MdfeEmissao>:
            // PagedResult<T> é classe, logo não é covariante, e mapear para o DTO
            // aqui evita mandar a entidade inteira (com XmlCompleto, que é text)
            // para a listagem.
            return new PagedResult<MdfeListItemDto>
            {
                CurrentPage = paginado.CurrentPage,
                PageCount = paginado.PageCount,
                PageSize = paginado.PageSize,
                RowCount = paginado.RowCount,
                Results = paginado.Results.Select(MapearLista).ToList()
            };
        }

        public async Task<MdfeResponseDto> GetByIdAsync(int id, int idCompany)
        {
            var mdfe = await _mdfeRepository.GetByIdAsync(id, idCompany);
            if (mdfe == null)
                throw new DomainException("Manifesto não encontrado.");

            return await MapearDetalheAsync(mdfe, idCompany);
        }

        /// <summary>
        /// Próximo número para o cabeçalho da tela. Não reserva — ver
        /// <see cref="IMdfeRepository.GetProximoNumeroAsync"/>.
        /// </summary>
        public async Task<MdfeProximoNumeroDto> GetProximoNumeroAsync(int idCompany)
        {
            var config = await ObterConfiguracaoFiscalAsync(idCompany);
            var serie = NormalizarSerie(config.NumeracaoDocumentos?.Mdfe?.Serie);

            return new MdfeProximoNumeroDto
            {
                Serie = serie,
                ProximoNumero = await _mdfeRepository.GetProximoNumeroAsync(
                    idCompany,
                    serie,
                    config.NumeracaoDocumentos?.Mdfe?.NumeroInicial ?? 1)
            };
        }

        // =====================================================================
        // Criação e edição do rascunho
        // =====================================================================

        public async Task<MdfeResponseDto> CriarAsync(MdfeCreateDto dto, int idCompany)
        {
            if (dto == null) throw new DomainException("Dados do manifesto não informados.");

            var config = await ObterConfiguracaoFiscalAsync(idCompany);
            var serie = NormalizarSerie(config.NumeracaoDocumentos?.Mdfe?.Serie);

            // O tipo de operação é conferido ANTES de qualquer outra coisa: é
            // barato, e é o que decide se a nota de entrada faz sentido neste
            // manifesto (RM14).
            await ValidarTipoOperacaoAsync(dto, idCompany, null);

            var mdfe = new MdfeEmissao
            {
                IdCompany = idCompany,
                Serie = serie,
                // Numero é atribuído pelo repositório, dentro da transação.
                DataEmissao = dto.DataEmissao,
                UfCarregamento = NormalizarUf(dto.UfCarregamento),
                UfDescarregamento = NormalizarUf(dto.UfDescarregamento),
                TipoEmitente = dto.TipoEmitente!.Value,
                Modal = dto.Modal!.Value,
                TipoOperacao = dto.TipoOperacao!.Value,
                StatusMdfe = MdfeStatus.Rascunho,
                // CodMunCarregamento/MunCarregamento não vêm do dto: são fixados
                // pelo consolidador, a partir da configuração fiscal.
                TipoCarga = dto.TipoCarga!.Value,
                ProdutoPredominante = dto.ProdutoPredominante.Trim(),
                InfoAdFisco = string.IsNullOrWhiteSpace(dto.InfoAdFisco) ? null : dto.InfoAdFisco.Trim(),
                InfoComplementar = string.IsNullOrWhiteSpace(dto.InfoComplementar) ? null : dto.InfoComplementar.Trim(),
                IndicadorPagamento = dto.IndicadorPagamento ?? MdfeIndicadorPagamento.AVista,
                PagamentoBanco = Limpar(dto.PagamentoBanco),
                PagamentoAgencia = Limpar(dto.PagamentoAgencia),
                PagamentoCnpjIpef = Limpar(dto.PagamentoCnpjIpef),
                PagamentoChavePix = Limpar(dto.PagamentoChavePix),
                CodigoCIOT = Limpar(dto.CodigoCIOT),
                ContratanteId = dto.ContratanteId,
                IdVeiculoTracao = dto.IdVeiculoTracao
            };

            AplicarFilhos(mdfe, dto);

            // O município de carregamento do payload é descartado aqui: o valor
            // vem da configuração fiscal, dentro do consolidador.
            ConsolidarTotais(mdfe, config);

            var criado = await _mdfeRepository.AddAsync(mdfe, config.NumeracaoDocumentos?.Mdfe?.NumeroInicial ?? 1);

            return await MapearDetalheAsync(criado, idCompany);
        }

        public async Task<MdfeResponseDto> AtualizarAsync(int id, MdfeUpdateDto dto, int idCompany)
        {
            if (dto == null) throw new DomainException("Dados do manifesto não informados.");

            // NÃO rastrear — só leitura para validação
            var existente = await _mdfeRepository.GetByIdAsync(id, idCompany);
            if (existente == null)
                throw new DomainException("Manifesto não encontrado.");

            if (!PodeEditar(existente))
                throw new DomainException(MotivoNaoPodeEditar(existente));

            await ValidarTipoOperacaoAsync(dto, idCompany, id);

            // Entidade NOVA, desconectada, que vai substituir a antiga.
            var mdfe = new MdfeEmissao
            {
                Id = id,
                IdCompany = idCompany,
                DataEmissao = dto.DataEmissao,
                UfCarregamento = NormalizarUf(dto.UfCarregamento),
                UfDescarregamento = NormalizarUf(dto.UfDescarregamento),
                TipoEmitente = dto.TipoEmitente!.Value,
                Modal = dto.Modal!.Value,
                TipoOperacao = dto.TipoOperacao!.Value,
                TipoCarga = dto.TipoCarga!.Value,
                ProdutoPredominante = dto.ProdutoPredominante.Trim(),
                InfoAdFisco = string.IsNullOrWhiteSpace(dto.InfoAdFisco) ? null : dto.InfoAdFisco.Trim(),
                InfoComplementar = string.IsNullOrWhiteSpace(dto.InfoComplementar) ? null : dto.InfoComplementar.Trim(),
                IndicadorPagamento = dto.IndicadorPagamento ?? MdfeIndicadorPagamento.AVista,
                PagamentoBanco = Limpar(dto.PagamentoBanco),
                PagamentoAgencia = Limpar(dto.PagamentoAgencia),
                PagamentoCnpjIpef = Limpar(dto.PagamentoCnpjIpef),
                PagamentoChavePix = Limpar(dto.PagamentoChavePix),
                CodigoCIOT = Limpar(dto.CodigoCIOT),
                ContratanteId = dto.ContratanteId,
                IdVeiculoTracao = dto.IdVeiculoTracao,
                // preservados pelo repositório a partir do registro atual:
                // Serie, Numero, CreatedAt, ChaveAcesso, XmlCompleto
            };

            AplicarFilhos(mdfe, dto);

            var config = await ObterConfiguracaoFiscalAsync(idCompany);
            ConsolidarTotais(mdfe, config);

            var atualizado = await _mdfeRepository.UpdateAsync(mdfe);

            if (existente.StatusMdfe != MdfeStatus.Rascunho)
            {
                await _mdfeRepository.ReabrirParaEdicaoAsync(id, idCompany);
                atualizado = await _mdfeRepository.GetByIdAsync(id, idCompany) ?? atualizado;
            }

            return await MapearDetalheAsync(atualizado, idCompany);
        }

        public async Task DeletarAsync(int id, int idCompany)
        {
            var mdfe = await _mdfeRepository.GetByIdAsync(id, idCompany);
            if (mdfe == null)
                throw new DomainException("Manifesto não encontrado.");

            if (!string.IsNullOrEmpty(mdfe.XmlCompleto))
                throw new DomainException("Este manifesto já foi transmitido e não pode ser excluído. Para corrigir os dados, edite o manifesto e gere o XML novamente; se ele já estiver autorizado, o caminho é o cancelamento.");

            await _mdfeRepository.DeleteAsync(id, idCompany);
        }

        // =====================================================================
        // Pesquisa de documentos (a área vermelha da tela)
        // =====================================================================

        /// <summary>
        /// Documentos fiscais manifestáveis, das duas origens, já com a
        /// elegibilidade resolvida (RM02/RM10).
        ///
        /// Documento inelegível CONTINUA na lista, com o motivo preenchido —
        /// esconder faria o usuário procurar uma nota que existe e não entender o
        /// silêncio.
        /// </summary>
        public async Task<List<DocumentoElegivelDto>> PesquisarDocumentosAsync(Filters filter, int idCompany)
        {
            filter ??= new Filters();

            var chavesVinculadas = await _mdfeRepository.GetChavesVinculadasAsync(idCompany, filter.DocumentoIgnorarMdfeId);

            var resultado = new List<DocumentoElegivelDto>();

            var querSaida = !filter.DocumentoTipo.HasValue || filter.DocumentoTipo.Value == MdfeTipoDocumento.Saida;
            var querEntrada = !filter.DocumentoTipo.HasValue || filter.DocumentoTipo.Value == MdfeTipoDocumento.Entrada;

            if (querSaida)
            {
                var notas = await _mdfeRepository.PesquisarNFeSaidaAsync(filter, idCompany);
                resultado.AddRange(notas.Select(n => MapearSaida(n, chavesVinculadas)));
            }

            if (querEntrada)
            {
                var compras = await _mdfeRepository.PesquisarNFeEntradaAsync(filter, idCompany);
                resultado.AddRange(compras.Select(c => MapearEntrada(c, chavesVinculadas)));
            }

            return resultado
                .OrderByDescending(d => d.DataEmissao)
                .ThenByDescending(d => d.Id)
                .ToList();
        }

        // =====================================================================
        // Geração do XML (monta → assina → valida → persiste)
        // =====================================================================

        /// <summary>
        /// Gera o XML do manifesto: monta pelo <see cref="MdfeBuilder"/>, assina
        /// com o certificado da configuração fiscal e valida contra os XSDs
        /// locais. O XML assinado é o produto desta fase.
        ///
        /// A ordem é obrigatória: <c>Valida()</c> só depois de <c>Assina()</c>,
        /// porque o schema exige o <c>ds:Signature</c> e o <c>cDV</c> — os dois
        /// calculados na assinatura. Validar antes reprovaria por falta deles.
        ///
        /// Falha de schema NÃO lança: marca o manifesto como
        /// <see cref="MdfeStatus.Erro"/> com a mensagem e devolve o estado, para a
        /// tela mostrar a pendência no próprio manifesto em vez de perder o
        /// rascunho. Erro de REGRA (documento já usado, veículo inapto) lança
        /// <see cref="DomainException"/>, porque aí o manifesto nem deveria ter
        /// chegado até aqui.
        /// </summary>
        public async Task<MdfeResponseDto> GerarXmlAsync(int id, int idCompany)
        {
            var mdfe = await _mdfeRepository.GetTrackedAsync(id, idCompany);
            if (mdfe == null)
                throw new DomainException("Manifesto não encontrado.");

            if (mdfe.StatusMdfe != MdfeStatus.Rascunho && mdfe.StatusMdfe != MdfeStatus.Erro)
                throw new DomainException($"O manifesto está com situação '{mdfe.StatusMdfe}' e não pode gerar XML novamente.");

            var config = await ObterConfiguracaoFiscalAsync(idCompany);

            // De novo aqui, e não só na criação/edição: o município de carregamento
            // é lido da configuração fiscal, e um manifesto gravado antes desta
            // regra (ou com a configuração corrigida depois) ainda tem o valor
            // antigo na linha. Este é o último ponto antes da assinatura, então é
            // onde a garantia de que o XML sai com o código certo vale de fato.
            // A persistência vai junto do XML, em SalvarXmlAsync.
            AplicarMunicipioDeCarregamento(mdfe, config);

            // ---- Regras de negócio (lançam) ----
            var validacao = await ValidarAsync(id, idCompany);
            if (!validacao.IsReady)
                throw new DomainException(string.Join(" ", validacao.PendingItems));

            var veiculos = await _mdfeRepository.GetVeiculosDoManifestoAsync(id, idCompany);

            string xml;
            string chave;

            try
            {
                var documento = MdfeBuilder.Construir(
                    mdfe,
                    config,
                    veiculos,
                    config.Ambiente == AmbienteEnum.Homologacao ? TipoAmbiente.Homologacao : TipoAmbiente.Producao);

                var configuracaoMdfe = MontarConfiguracaoMdfe(config);

                // Assina primeiro: é o Assina() que calcula o cDV e o Id, e é ele
                // que fixa a versão do leiaute no XML.
                documento.Assina(null, null, configuracaoMdfe);

                chave = documento.InfMDFe.Id.Substring(4); // "MDFe" + 44 dígitos

                documento.Valida(configuracaoMdfe);

                xml = documento.XmlString();
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Falha de montagem/assinatura/schema: o rascunho é preservado e o
                // motivo fica gravado. Não relançamos como DomainException porque
                // não é erro do usuário — é o XML que não passou, e a mensagem do
                // validador é o que interessa.
                var mensagem = MensagemDeValidacao(ex);
                await _mdfeRepository.MarcarErroAsync(id, idCompany, mensagem);

                throw new DomainException($"Não foi possível gerar o XML do manifesto: {mensagem}");
            }

            await _mdfeRepository.SalvarXmlAsync(
                id, idCompany, chave, xml,
                mdfe.ValorTotal, mdfe.PesoBruto, mdfe.QuantidadeNFe,
                mdfe.CodMunCarregamento, mdfe.MunCarregamento);

            // VehicleUsageHistory: o consumidor que a migration de veículo deixou
            // pendente ("SourceId nulo para MDF-e até que a tabela de manifesto
            // seja criada"). Grava uma linha por veículo do comboio.
            mdfe.ChaveAcesso = chave;
            await _mdfeRepository.RegistrarUsoDosVeiculosAsync(mdfe);

            var atualizado = await _mdfeRepository.GetByIdAsync(id, idCompany);
            return await MapearDetalheAsync(atualizado!, idCompany);
        }

        /// <summary>
        /// XML assinado, para download. Devolve os bytes e o nome do arquivo.
        /// </summary>
        public async Task<(byte[] Bytes, string NomeArquivo)> ObterXmlAsync(int id, int idCompany)
        {
            var mdfe = await _mdfeRepository.GetByIdAsync(id, idCompany);
            if (mdfe == null)
                throw new DomainException("Manifesto não encontrado.");

            if (string.IsNullOrEmpty(mdfe.XmlCompleto))
                throw new DomainException("Este manifesto ainda não teve o XML gerado.");

            var nome = $"{mdfe.Serie}-{mdfe.Numero}-mdfe.xml";
            return (Encoding.UTF8.GetBytes(mdfe.XmlCompleto), nome);
        }

        // =====================================================================
        // Transmissão e eventos
        //
        // O MDF-e é SÍNCRONO: não há lote, recibo nem polling como na NF-e. O
        // envio já devolve o protocolo (ou a rejeição), então "transmitir" e
        // "consultar o recibo" são a mesma chamada — o que a consulta de situação
        // resolve aqui é outro caso: saber o que a SEFAZ pensa de um manifesto
        // cuja resposta se perdeu no caminho.
        // =====================================================================

        /// <summary>
        /// Transmite o manifesto à SEFAZ (<c>MDFeRecepcaoSinc</c>) e grava o
        /// resultado.
        ///
        /// Parte do XML <b>já assinado e validado</b> que está em
        /// <see cref="MdfeEmissao.XmlCompleto"/>, e não de uma remontagem: o que
        /// sobe é o documento que passou no validador local. Não há risco de
        /// divergência por causa disso — a biblioteca re-assina dentro do
        /// <c>MDFeRecepcaoSinc</c>, mas o <c>Assina()</c> dela <b>atribui</b> a
        /// assinatura em vez de acumular, e reescreve os mesmos valores que o
        /// <c>MdfeBuilder</c> já tinha gravado (<c>versaoModal</c>, QR Code, Id,
        /// cDV). Como o RSA PKCS#1 v1.5 é determinístico, o documento que sobe é
        /// equivalente ao que foi validado — e a própria biblioteca valida de novo
        /// contra os XSDs antes de enviar.
        ///
        /// <b>Nenhuma exceção de rede escapa como exceção crua:</b> falha de
        /// comunicação vira <see cref="DomainException"/> com o motivo e o
        /// manifesto continua <see cref="MdfeStatus.Validado"/>, pronto para uma
        /// nova tentativa. Só a resposta da SEFAZ muda o estado.
        /// </summary>
        public async Task<MdfeResponseDto> TransmitirAsync(int id, int idCompany)
        {
            var mdfe = await _mdfeRepository.GetTrackedAsync(id, idCompany);
            if (mdfe == null)
                throw new DomainException("Manifesto não encontrado.");

            if (string.IsNullOrEmpty(mdfe.XmlCompleto))
                throw new DomainException("Este manifesto ainda não teve o XML gerado. Gere e valide o XML antes de transmitir.");

            if (mdfe.StatusMdfe == MdfeStatus.Autorizado)
                throw new DomainException($"Este manifesto já está autorizado (protocolo {mdfe.Protocolo}). Não há o que transmitir.");

            if (mdfe.StatusMdfe == MdfeStatus.Cancelado || mdfe.StatusMdfe == MdfeStatus.Encerrado)
                throw new DomainException($"Este manifesto está '{mdfe.StatusMdfe}' e não pode mais ser transmitido.");

            // Rejeição anterior: o XML foi assinado e o número consumido, então a
            // única saída útil é tentar de novo depois de entender o motivo —
            // e é exatamente o que reenviar faz. Gerar o XML de novo é que não
            // faz sentido (o número já foi usado).
            if (mdfe.StatusMdfe != MdfeStatus.Validado && mdfe.StatusMdfe != MdfeStatus.Erro)
                throw new DomainException($"O manifesto está com situação '{mdfe.StatusMdfe}' e não pode ser transmitido. Gere o XML primeiro.");

            var config = await ObterConfiguracaoFiscalAsync(idCompany);
            var configuracaoMdfe = MontarConfiguracaoMdfe(config);

            // O servidor web não sai gravando XML em disco: a biblioteca chama
            // SalvarXmlEmDisco no meio do caminho, e é este flag que a faz parar.
            configuracaoMdfe.IsSalvarXml = false;

            MDFe.Classes.Retorno.MDFeRetRecepcao.Sincrono.MDFeRetMDFe retorno;

            try
            {
                var documento = ComVersaoDoDocumento(FuncoesXml.XmlStringParaClasse<MDFEletronico>(mdfe.XmlCompleto));
                retorno = new MDFe.Servicos.RecepcaoMDFe.ServicoMDFeRecepcao()
                    .MDFeRecepcaoSinc(documento, configuracaoMdfe);
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Falha local ou de comunicação: o manifesto NÃO muda de estado.
                // Rebaixá-lo para Erro aqui faria a tela dizer que o manifesto
                // está ruim quando o que está fora é a rede ou a SEFAZ.
                var mensagem = MensagemDeValidacao(ex);
                await _mdfeRepository.RegistrarFalhaDeTransmissaoAsync(id, idCompany, mensagem);

                throw new DomainException($"Não foi possível transmitir o manifesto: {mensagem}");
            }

            var resposta = SerializarResposta(retorno);

            if (retorno.CStat == 100)
            {
                var protocolo = retorno.ProtMdFe?.InfProt;

                // procMDFe = MDFe + protMDFe. É o documento que a SEFAZ devolve
                // autorizado, e o que a DAMDFe e os eventos precisam ler (os dois
                // exigem o protocolo). O MDFe continua dentro, então substituir o
                // XmlCompleto não perde o manifesto assinado.
                var proc = new MDFe.Classes.Retorno.MDFeProcMDFe
                {
                    // Do builder, e não o padrão do construtor, que é Versao100:
                    // o procMDFe de um manifesto v3.00 declarado como 1.00 é
                    // simplesmente o leiaute errado.
                    Versao = MdfeBuilder.VersaoLayout,
                    MDFe = ComVersaoDoDocumento(FuncoesXml.XmlStringParaClasse<MDFEletronico>(mdfe.XmlCompleto)),
                    ProtMDFe = retorno.ProtMdFe
                };

                await _mdfeRepository.RegistrarAutorizacaoAsync(
                    id, idCompany,
                    retorno.CStat,
                    retorno.XMotivo,
                    protocolo?.NProt,
                    retorno.NRec,
                    DataDoProtocolo(protocolo?.DhRecbto),
                    FuncoesXml.ClasseParaXmlString(proc),
                    resposta);
            }
            else
            {
                await _mdfeRepository.RegistrarRejeicaoAsync(
                    id, idCompany, retorno.CStat, retorno.XMotivo, resposta);
            }

            var atualizado = await _mdfeRepository.GetByIdAsync(id, idCompany);
            return await MapearDetalheAsync(atualizado!, idCompany);
        }

        /// <summary>
        /// Consulta a situação do manifesto na SEFAZ pela chave
        /// (<c>MDFeConsultaProtocolo</c>).
        ///
        /// Serve para o caso em que a resposta da transmissão se perdeu: o
        /// manifesto pode ter sido autorizado lá e continuar "Validado" aqui. Se a
        /// consulta revelar autorização, o estado local é sincronizado — e é por
        /// isso que não basta gravar o <c>cStat</c> e seguir.
        /// </summary>
        public async Task<MdfeResponseDto> ConsultarSituacaoAsync(int id, int idCompany)
        {
            var mdfe = await _mdfeRepository.GetByIdAsync(id, idCompany);
            if (mdfe == null)
                throw new DomainException("Manifesto não encontrado.");

            if (string.IsNullOrWhiteSpace(mdfe.ChaveAcesso))
                throw new DomainException("Este manifesto ainda não teve o XML gerado — não há chave para consultar.");

            var config = await ObterConfiguracaoFiscalAsync(idCompany);
            var configuracaoMdfe = MontarConfiguracaoMdfe(config);
            configuracaoMdfe.IsSalvarXml = false;

            MDFe.Classes.Retorno.MDFeConsultaProtocolo.MDFeRetConsSitMDFe retorno;

            try
            {
                retorno = new MDFe.Servicos.ConsultaProtocoloMDFe.ServicoMDFeConsultaProtocolo()
                    .MDFeConsultaProtocolo(mdfe.ChaveAcesso!, configuracaoMdfe);
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var mensagem = MensagemDeValidacao(ex);
                throw new DomainException($"Não foi possível consultar a situação do manifesto: {mensagem}");
            }

            var resposta = SerializarResposta(retorno);
            var protocolo = retorno.ProtMDFe?.InfProt;

            // Só sincroniza o que ainda não estava autorizado aqui. Numa consulta
            // de rotina a um manifesto já autorizado, `RegistrarAutorizacaoAsync`
            // reescreveria o protocolo e somaria em `TryCount` — e o contador é de
            // TENTATIVAS DE TRANSMISSÃO; vê-lo subir a cada consulta faria a tela
            // mentir sobre o histórico do documento.
            var jaAutorizado = mdfe.StatusMdfe == MdfeStatus.Autorizado
                               || mdfe.StatusMdfe == MdfeStatus.Encerrado
                               || mdfe.StatusMdfe == MdfeStatus.Cancelado;

            if ((retorno.CStat == 100 || protocolo?.CStat == 100) && !jaAutorizado)
            {
                var proc = new MDFe.Classes.Retorno.MDFeProcMDFe
                {
                    Versao = MdfeBuilder.VersaoLayout,
                    MDFe = ComVersaoDoDocumento(FuncoesXml.XmlStringParaClasse<MDFEletronico>(mdfe.XmlCompleto!)),
                    ProtMDFe = retorno.ProtMDFe
                };

                await _mdfeRepository.RegistrarAutorizacaoAsync(
                    id, idCompany,
                    100,
                    retorno.XMotivo ?? protocolo?.XMotivo,
                    protocolo?.NProt,
                    null,
                    DataDoProtocolo(protocolo?.DhRecbto),
                    FuncoesXml.ClasseParaXmlString(proc),
                    resposta);
            }
            else
            {
                await _mdfeRepository.RegistrarConsultaAsync(id, idCompany, retorno.CStat, retorno.XMotivo, resposta);
            }

            var atualizado = await _mdfeRepository.GetByIdAsync(id, idCompany);
            return await MapearDetalheAsync(atualizado!, idCompany);
        }

        /// <summary>
        /// Situação do serviço do MDF-e na SEFAZ (<c>consStatServMDFe</c>).
        ///
        /// Não toca em manifesto nenhum — é a checagem que se faz antes de
        /// transmitir. Com o serviço parado (ou em manutenção), a transmissão falha
        /// de um jeito que parece erro do manifesto; esta rota existe para separar
        /// as duas coisas.
        /// </summary>
        public async Task<MdfeStatusServicoDto> ConsultarStatusServicoAsync(int idCompany)
        {
            var config = await ObterConfiguracaoFiscalAsync(idCompany);
            var configuracaoMdfe = MontarConfiguracaoMdfe(config);
            configuracaoMdfe.IsSalvarXml = false;

            MDFe.Classes.Retorno.MDFeStatusServico.MDFeRetConsStatServ retorno;

            try
            {
                retorno = new MDFe.Servicos.StatusServicoMDFe.ServicoMDFeStatusServico()
                    .MDFeStatusServico(configuracaoMdfe);
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var mensagem = MensagemDeValidacao(ex);
                throw new DomainException($"Não foi possível consultar a situação do serviço do MDF-e: {mensagem}");
            }

            return new MdfeStatusServicoDto
            {
                CStat = retorno.CStat,
                XMotivo = retorno.XMotivo,
                DhRecbto = retorno.DhRecbto,
                TMed = retorno.TMed,
                XObs = retorno.XObs,
                VersaoAplicativo = retorno.VerAplic,
                Ambiente = retorno.TpAmb.ToString()
            };
        }

        /// <summary>
        /// Encerra o manifesto (<c>evEncMDFe</c>). É obrigatório ao fim da viagem:
        /// manifesto não encerrado sujeita o transportador a multa.
        ///
        /// UF e município do encerramento vêm no DTO; quando não vêm, o serviço usa
        /// o município de DESCARGA do manifesto — que é o palpite certo na maioria
        /// dos casos, e está explícito aqui em vez de escondido.
        /// </summary>
        public async Task<MdfeResponseDto> EncerrarAsync(int id, int idCompany, MdfeEncerrarDto dto)
        {
            var mdfe = await _mdfeRepository.GetTrackedAsync(id, idCompany);
            if (mdfe == null)
                throw new DomainException("Manifesto não encontrado.");

            if (mdfe.StatusMdfe != MdfeStatus.Autorizado)
                throw new DomainException($"Só manifesto autorizado pode ser encerrado — este está '{mdfe.StatusMdfe}'.");

            if (string.IsNullOrWhiteSpace(mdfe.Protocolo))
                throw new DomainException("O manifesto está autorizado mas sem protocolo gravado — encerrar sem ele seria recusado pela SEFAZ. Consulte a situação do manifesto.");

            var documento = CarregarDocumentoAutorizado(mdfe);

            var uf = string.IsNullOrWhiteSpace(dto?.UfEncerramento)
                ? mdfe.UfDescarregamento
                : NormalizarUf(dto!.UfEncerramento!);

            // A UF é resolvida aqui, e não dentro do try lá embaixo: uma sigla
            // inválida tem que virar "informe uma UF válida", e não a mensagem de
            // ArgumentException do Enum.Parse. O enum de UF é fechado (as 27), e
            // `UfDescarregamento` já está normalizado pelo serviço na criação.
            if (!Enum.TryParse<Estado>(uf, out var ufEncerramento))
                throw new DomainException($"'{uf}' não é uma UF válida para o encerramento.");

            var codigoMunicipio = ResolverMunicipioDeEncerramento(mdfe, dto);

            var config = await ObterConfiguracaoFiscalAsync(idCompany);
            var configuracaoMdfe = MontarConfiguracaoMdfe(config);
            configuracaoMdfe.IsSalvarXml = false;

            MDFe.Classes.Retorno.MDFeEvento.MDFeRetEventoMDFe retorno;

            try
            {
                retorno = new MDFe.Servicos.EventosMDFe.ServicoMDFeEvento()
                    .MDFeEventoEncerramentoMDFeEventoEncerramento(
                        documento,
                        ufEncerramento,
                        codigoMunicipio,
                        (byte)(mdfe.SequenciaEvento + 1),
                        mdfe.Protocolo!,
                        configuracaoMdfe);
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var mensagem = MensagemDeValidacao(ex);
                throw new DomainException($"Não foi possível encerrar o manifesto: {mensagem}");
            }

            var resposta = SerializarResposta(retorno);
            var infEvento = retorno.InfEvento;

            if (infEvento == null || infEvento.CStat != 135)
                throw new DomainException(
                    $"A SEFAZ recusou o encerramento ({infEvento?.CStat}): {infEvento?.XMotivo ?? retorno.RetornoXmlString}");

            await _mdfeRepository.RegistrarEncerramentoAsync(
                id, idCompany,
                infEvento.NProt,
                infEvento.DhRegEvento ?? DateTime.UtcNow,
                infEvento.NSeqEvento,
                resposta);

            var atualizado = await _mdfeRepository.GetByIdAsync(id, idCompany);
            return await MapearDetalheAsync(atualizado!, idCompany);
        }

        /// <summary>
        /// Cancela o manifesto (<c>evCancMDFe</c>).
        ///
        /// A janela de 24 horas é conferida AQUI, contra
        /// <see cref="MdfeEmissao.DataAutorizacao"/>, e não deixada para a SEFAZ:
        /// descobrir que o prazo passou só depois de montar e assinar o evento dá
        /// ao usuário um erro que ele não pode resolver. A justificativa também é
        /// validada aqui (15 a 255), pelo mesmo motivo.
        /// </summary>
        public async Task<MdfeResponseDto> CancelarAsync(int id, int idCompany, MdfeCancelarDto dto)
        {
            var mdfe = await _mdfeRepository.GetTrackedAsync(id, idCompany);
            if (mdfe == null)
                throw new DomainException("Manifesto não encontrado.");

            if (dto == null)
                throw new DomainException("Dados do cancelamento não informados.");

            if (mdfe.StatusMdfe != MdfeStatus.Autorizado)
                throw new DomainException($"Só manifesto autorizado pode ser cancelado — este está '{mdfe.StatusMdfe}'.");

            if (string.IsNullOrWhiteSpace(mdfe.Protocolo))
                throw new DomainException("O manifesto está autorizado mas sem protocolo gravado — cancelar sem ele seria recusado pela SEFAZ. Consulte a situação do manifesto.");

            var justificativa = (dto.Justificativa ?? string.Empty).Trim();
            if (justificativa.Length < 15 || justificativa.Length > 255)
                throw new DomainException("A justificativa do cancelamento deve ter de 15 a 255 caracteres.");

            if (mdfe.DataAutorizacao.HasValue)
            {
                var prazo = mdfe.DataAutorizacao.Value.AddHours(24);
                if (DateTime.UtcNow > prazo)
                    throw new DomainException(
                        $"O prazo de cancelamento venceu em {prazo:dd/MM/yyyy HH:mm} (24 horas após a autorização). " +
                        "Passado o prazo, o manifesto segue válido e o caminho é o encerramento.");
            }

            var documento = CarregarDocumentoAutorizado(mdfe);

            var config = await ObterConfiguracaoFiscalAsync(idCompany);
            var configuracaoMdfe = MontarConfiguracaoMdfe(config);
            configuracaoMdfe.IsSalvarXml = false;

            MDFe.Classes.Retorno.MDFeEvento.MDFeRetEventoMDFe retorno;

            try
            {
                retorno = new MDFe.Servicos.EventosMDFe.ServicoMDFeEvento()
                    .MDFeEventoCancelar(
                        documento,
                        (byte)(mdfe.SequenciaEvento + 1),
                        mdfe.Protocolo!,
                        justificativa,
                        configuracaoMdfe);
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var mensagem = MensagemDeValidacao(ex);
                throw new DomainException($"Não foi possível cancelar o manifesto: {mensagem}");
            }

            var resposta = SerializarResposta(retorno);
            var infEvento = retorno.InfEvento;

            // 135 = evento registrado e vinculado ao documento; 136 = registrado,
            // mas não vinculado (o cancelamento fora do prazo cai aqui). Os dois
            // são "aceito" — tratar só o 135 como sucesso faria um cancelamento
            // legítimo parecer falha.
            if (infEvento == null || (infEvento.CStat != 135 && infEvento.CStat != 136))
                throw new DomainException(
                    $"A SEFAZ recusou o cancelamento ({infEvento?.CStat}): {infEvento?.XMotivo ?? retorno.RetornoXmlString}");

            await _mdfeRepository.RegistrarCancelamentoAsync(
                id, idCompany,
                infEvento.DhRegEvento ?? DateTime.UtcNow,
                justificativa,
                infEvento.NSeqEvento,
                resposta);

            var atualizado = await _mdfeRepository.GetByIdAsync(id, idCompany);
            return await MapearDetalheAsync(atualizado!, idCompany);
        }

        /// <summary>
        /// DAMDFe em PDF. Só de manifesto AUTORIZADO: a DAMDFe é o documento
        /// auxiliar do manifesto protocolado, e imprimir de um rascunho daria ao
        /// motorista uma folha que não corresponde a nada na SEFAZ.
        /// </summary>
        public async Task<(byte[] Bytes, string NomeArquivo)> ObterDamdfeAsync(int id, int idCompany)
        {
            var mdfe = await _mdfeRepository.GetByIdAsync(id, idCompany);
            if (mdfe == null)
                throw new DomainException("Manifesto não encontrado.");

            if (mdfe.StatusMdfe != MdfeStatus.Autorizado && mdfe.StatusMdfe != MdfeStatus.Encerrado && mdfe.StatusMdfe != MdfeStatus.Cancelado)
                throw new DomainException("A DAMDFe só pode ser impressa depois da autorização do manifesto.");

            if (string.IsNullOrWhiteSpace(mdfe.XmlCompleto) || string.IsNullOrWhiteSpace(mdfe.ChaveAcesso))
                throw new DomainException("Este manifesto não tem o XML autorizado gravado. Consulte a situação do manifesto.");

            var config = await ObterConfiguracaoFiscalAsync(idCompany);

            // A tarja não sai do XML: o documento continua sendo um manifesto
            // autorizado depois de cancelado ou encerrado, e um DAMDFe sem tarja
            // circulando na estrada é exatamente o que o cancelamento existe para
            // evitar. Quem sabe o estado é o banco, então é daqui que ela vem.
            var tarja = mdfe.StatusMdfe switch
            {
                MdfeStatus.Cancelado => "CANCELADO",
                MdfeStatus.Encerrado => "ENCERRADO",
                _ => null
            };

            var danfe = new MDFe.Damdfe.QuestPdf.ImpressaoMdfe.DamdfeMdfeDocument(
                mdfe.XmlCompleto!,
                config.Emitente?.Logo,
                tarja);

            var nome = $"{mdfe.Serie}-{mdfe.Numero}-damdfe.pdf";
            return (danfe.GerarPdfBytes(), nome);
        }

        /// <summary>
        /// Devolve o manifesto desserializado com a versão do leiaute do PRÓPRIO
        /// documento propagada para o <c>ide</c>.
        ///
        /// <b>Sem isto a transmissão não funciona.</b> O <c>XmlSerializer</c> cria o
        /// <c>MDFeIde</c> pelo construtor privado de serialização, que não recebe a
        /// versão; os proxies <c>dhEmi</c>/<c>dhIniViagem</c> caem então no fallback
        /// para o singleton <c>MDFeConfiguracao.Instancia</c>, que neste serviço nunca
        /// é configurado (é global e compartilhado entre empresas). O valor fica
        /// <c>0</c> e a serialização lança "Versão Inválida para MDF-e" — dentro de
        /// <c>MDFeRecepcaoSinc</c>, ao montar o envelope SOAP, longe daqui.
        ///
        /// A versão vem de <c>infMDFe/@versao</c>, que é o que o documento declara
        /// sobre si mesmo: é ela que diz em que formato as datas foram escritas e,
        /// portanto, em que formato devem ser reescritas. Não é constante
        /// (<see cref="MdfeBuilder.VersaoLayout"/>) de propósito — um manifesto
        /// antigo em v1.00 continuaria sendo reescrito no formato errado.
        /// </summary>
        private static MDFEletronico ComVersaoDoDocumento(MDFEletronico documento)
        {
            documento.InfMDFe.Ide.VersaoLayout = documento.InfMDFe.Versao;
            return documento;
        }

        /// <summary>
        /// Carrega o <c>MDFe</c> de dentro do que está gravado em
        /// <c>XmlCompleto</c>.
        ///
        /// Depois da autorização o que está lá é o <c>procMDFe</c> (manifesto +
        /// protocolo), não o <c>MDFe</c> solto — e os eventos precisam do
        /// <c>MDFe</c>. Ler direto como <c>MDFEletronico</c> falharia com um
        /// documento autorizado, que é justamente o único caso em que se encerra ou
        /// cancela. Por isso a raiz é conferida antes.
        /// </summary>
        private static MDFEletronico CarregarDocumentoAutorizado(MdfeEmissao mdfe)
        {
            if (string.IsNullOrWhiteSpace(mdfe.XmlCompleto))
                throw new DomainException("Este manifesto não tem o XML gravado.");

            if (mdfe.XmlCompleto!.Contains("<mdfeProc"))
            {
                var proc = FuncoesXml.XmlStringParaClasse<MDFe.Classes.Retorno.MDFeProcMDFe>(mdfe.XmlCompleto);
                if (proc?.MDFe == null)
                    throw new DomainException("O XML autorizado do manifesto está sem o nó MDFe.");

                return ComVersaoDoDocumento(proc.MDFe);
            }

            return ComVersaoDoDocumento(FuncoesXml.XmlStringParaClasse<MDFEletronico>(mdfe.XmlCompleto));
        }

        /// <summary>
        /// Município de encerramento: o do DTO quando informado, senão o primeiro
        /// município de DESCARGA do manifesto (é onde a carga foi entregue, e
        /// portanto onde a viagem terminou na maioria dos casos).
        /// </summary>
        private static long ResolverMunicipioDeEncerramento(MdfeEmissao mdfe, MdfeEncerrarDto? dto)
        {
            if (!string.IsNullOrWhiteSpace(dto?.CodigoMunicipioEncerramento))
            {
                var digitos = SomenteDigitos(dto!.CodigoMunicipioEncerramento);

                if (digitos?.Length != 7)
                    throw new DomainException("O código IBGE do município de encerramento precisa ter 7 dígitos.");

                return long.Parse(digitos);
            }

            var municipioDescarga = mdfe.Documentos?
                .Select(d => SomenteDigitos(d.CodMunDescarga))
                .FirstOrDefault(c => c?.Length == 7);

            if (municipioDescarga == null)
                throw new DomainException(
                    "Não há município de descarga no manifesto para usar como município de encerramento. " +
                    "Informe o município de encerramento.");

            return long.Parse(municipioDescarga);
        }

        /// <summary>
        /// O retorno cru da SEFAZ, para diagnóstico. <c>RetornoXmlString</c> é o que
        /// interessa quando a rejeição não vem acompanhada de motivo legível — sem
        /// ele, sobra só o <c>cStat</c>.
        /// </summary>
        private static string SerializarResposta(object? retorno)
        {
            if (retorno == null)
                return string.Empty;

            try
            {
                return FuncoesXml.ClasseParaXmlString(retorno);
            }
            catch (Exception)
            {
                // A resposta crua é diagnóstico, não parte do fluxo: se ela não
                // serializar, o manifesto não pode deixar de ser gravado por isso.
                return retorno.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        /// Data de autorização a partir do <c>dhRecbto</c> do protocolo.
        ///
        /// O <c>DateTime</c> do retorno não é anulável, então "sem data" chega
        /// como <see cref="DateTime.MinValue"/> — e gravar isso deixaria a janela
        /// de 24h do cancelamento vencida desde sempre, recusando todo
        /// cancelamento. O <c>UtcNow</c> é o palpite certo para um documento que a
        /// SEFAZ acabou de protocolar.
        /// </summary>
        private static DateTime DataDoProtocolo(DateTime? data)
        {
            return data.GetValueOrDefault() == default ? DateTime.UtcNow : data!.Value;
        }

        // =====================================================================
        // Validação (RM01–RM14)
        // =====================================================================

        /// <summary>
        /// Checklist completo do manifesto. Devolve TODAS as pendências, e não só
        /// a primeira — corrigir em uma passada, e não descobrir um item por
        /// tentativa.
        ///
        /// Os avisos (<see cref="MdfeValidacaoResult.Warnings"/>) NÃO bloqueiam:
        /// hoje só a RM11 (peso acima da capacidade), porque o MDF-e não recusa
        /// por excesso de peso — quem recusa é a balança da fiscalização.
        /// </summary>
        public async Task<MdfeValidacaoResult> ValidarAsync(int id, int idCompany)
        {
            var mdfe = await _mdfeRepository.GetByIdAsync(id, idCompany);
            if (mdfe == null)
                throw new DomainException("Manifesto não encontrado.");

            return await CalcularValidacaoAsync(mdfe, idCompany);
        }

        private async Task<MdfeValidacaoResult> CalcularValidacaoAsync(MdfeEmissao mdfe, int idCompany)
        {
            var pendencias = new List<string>();
            var avisos = new List<string>();

            // RM01 — ao menos um documento.
            if (mdfe.Documentos == null || mdfe.Documentos.Count == 0)
                pendencias.Add("Vincule ao menos uma NF-e ao manifesto.");

            // RM02 — chave de 44 dígitos em todo documento, e município de descarga
            // (RM03) informado. Sem o município o XML não tem como agrupar.
            foreach (var doc in mdfe.Documentos ?? new List<MdfeDocumento>())
            {
                var digitos = SomenteDigitos(doc.ChaveNFe);
                if (digitos == null || digitos.Length != 44)
                    pendencias.Add($"A chave da NF-e '{doc.ChaveNFe}' não tem 44 dígitos.");

                if (string.IsNullOrWhiteSpace(doc.CodMunDescarga) || SomenteDigitos(doc.CodMunDescarga)?.Length != 7)
                    pendencias.Add($"Informe o código IBGE do município de descarga da NF-e {(digitos?.Length == 44 ? digitos.Substring(25, 9) : doc.ChaveNFe)}.");

                if (string.IsNullOrWhiteSpace(doc.MunicipioDescarga))
                    pendencias.Add($"Informe o nome do município de descarga da NF-e {doc.ChaveNFe}.");
            }

            // RM04 — a UF de descarregamento declarada tem de bater com os destinos.
            if (!string.IsNullOrWhiteSpace(mdfe.UfDescarregamento) && (mdfe.Documentos?.Count ?? 0) > 0)
            {
                var ufFim = NormalizarUf(mdfe.UfDescarregamento);
                var divergentes = (mdfe.Documentos ?? new List<MdfeDocumento>())
                    .Where(d => !string.IsNullOrWhiteSpace(d.UfDestino) && NormalizarUf(d.UfDestino) != ufFim)
                    .Select(d => d.UfDestino)
                    .Distinct()
                    .ToList();

                if (divergentes.Count > 0)
                    pendencias.Add($"A UF de descarregamento do manifesto ({ufFim}) não corresponde à UF de destino das NF-e ({string.Join(", ", divergentes)}).");
            }

            // RM12 — UFs contra a lista real do IBGE, não contra um literal.
            if (!UfList.IsValid(mdfe.UfCarregamento))
                pendencias.Add("UF de carregamento não informada ou inválida.");

            if (!UfList.IsValid(mdfe.UfDescarregamento))
                pendencias.Add("UF de descarregamento não informada ou inválida.");

            // RM13 — percurso: UFs válidas, distintas entre si e diferentes das
            // UFs de início e fim (repetir o trajeto reprova no leiaute).
            var percurso = (mdfe.Percurso ?? new List<MdfePercurso>()).ToList();
            var ufsPercurso = new List<string>();
            foreach (var p in percurso)
            {
                if (!UfList.IsValid(p.UfPercurso))
                {
                    pendencias.Add($"UF de percurso inválida: '{p.UfPercurso}'.");
                    continue;
                }

                var uf = NormalizarUf(p.UfPercurso);
                if (ufsPercurso.Contains(uf))
                    pendencias.Add($"A UF {uf} aparece mais de uma vez no percurso.");
                else
                    ufsPercurso.Add(uf);
            }

            foreach (var uf in ufsPercurso)
            {
                if (uf == NormalizarUf(mdfe.UfCarregamento) || uf == NormalizarUf(mdfe.UfDescarregamento))
                    pendencias.Add($"A UF {uf} está no percurso e também é UF de carregamento ou descarregamento.");
            }

            // Município de carregamento — obrigatório no leiaute.
            if (SomenteDigitos(mdfe.CodMunCarregamento)?.Length != 7)
                pendencias.Add("Informe o código IBGE do município de carregamento (7 dígitos).");

            if (string.IsNullOrWhiteSpace(mdfe.MunCarregamento))
                pendencias.Add("Informe o nome do município de carregamento.");

            if (string.IsNullOrWhiteSpace(mdfe.ProdutoPredominante))
                pendencias.Add("Informe o produto predominante da carga.");

            // RM05/RM07 — veículo de tração presente e APTO. Reusa o checklist do
            // VehicleService em vez de reimplementá-lo: duas cópias das mesmas
            // seis regras divergiriam.
            var veiculos = await _mdfeRepository.GetVeiculosDoManifestoAsync(mdfe.Id, idCompany);

            if (veiculos.Count == 0)
            {
                pendencias.Add("Escalone ao menos um veículo (tração).");
            }
            else
            {
                foreach (var veiculo in veiculos)
                {
                    var status = _vehicleService.CalculateMdfEStatus(veiculo);
                    if (!status.IsReady)
                        pendencias.Add($"Veículo {veiculo.LicensePlate}: {string.Join(" ", status.PendingItems)}");

                    // O reboque tem exigência que o checklist do cadastro não faz:
                    // no leiaute o capKG do reboque é obrigatório (no veículo de
                    // tração é opcional). Descoberto aqui, e não na SEFAZ.
                    if (veiculo.VehicleType == VehicleType.Trailer && (veiculo.CapacityKg ?? 0) <= 0)
                        pendencias.Add($"O reboque {veiculo.LicensePlate} não tem capacidade (KG) cadastrada, e o leiaute exige no reboque.");
                }

                var reboques = veiculos.Count(v => v.VehicleType == VehicleType.Trailer);
                if (reboques > MdfeBuilder.MaxReboques)
                    pendencias.Add($"O comboio tem {reboques} reboques e o leiaute aceita no máximo {MdfeBuilder.MaxReboques}.");
            }

            // RM06 — ao menos um condutor, com nome e CPF de 11 dígitos.
            var condutores = (mdfe.Condutores ?? new List<MdfeCondutor>()).ToList();
            if (condutores.Count == 0)
            {
                pendencias.Add("Informe ao menos um condutor.");
            }
            else
            {
                foreach (var condutor in condutores)
                {
                    if (string.IsNullOrWhiteSpace(condutor.Nome))
                        pendencias.Add("Há condutor sem nome informado.");

                    if (SomenteDigitos(condutor.Cpf)?.Length != 11)
                        pendencias.Add($"O condutor '{(string.IsNullOrWhiteSpace(condutor.Nome) ? condutor.Cpf : condutor.Nome)}' está sem CPF válido (11 dígitos).");
                }

                if (condutores.Count > MdfeBuilder.MaxCondutores)
                    pendencias.Add($"O manifesto tem {condutores.Count} condutores e o leiaute aceita no máximo {MdfeBuilder.MaxCondutores}.");
            }

            // RM08 — PST exige RNTRC do emitente e contratante identificado.
            var config = await ObterConfiguracaoFiscalAsync(idCompany);

            if (mdfe.TipoEmitente == MdfeTipoEmitente.Pst)
            {
                if (string.IsNullOrWhiteSpace(config.Emitente?.Rntrc))
                    pendencias.Add("MDF-e de Prestador de Serviço de Transporte exige o RNTRC do emitente — preencha na configuração fiscal da empresa.");

                if (mdfe.ContratanteId == null)
                    pendencias.Add("MDF-e de Prestador de Serviço de Transporte exige o contratante do transporte.");

                if (string.IsNullOrWhiteSpace(mdfe.CodigoCIOT))
                    pendencias.Add("MDF-e de Prestador de Serviço de Transporte exige o CIOT.");

                // infBanc é obrigatório quando infPag é emitido: ou o destino vem
                // completo, ou o grupo de pagamento não é montado.
                var ramos = new List<string>();
                if (!string.IsNullOrWhiteSpace(mdfe.PagamentoBanco) || !string.IsNullOrWhiteSpace(mdfe.PagamentoAgencia)) ramos.Add("banco/agência");
                if (!string.IsNullOrWhiteSpace(mdfe.PagamentoCnpjIpef)) ramos.Add("CNPJ IPEF");
                if (!string.IsNullOrWhiteSpace(mdfe.PagamentoChavePix)) ramos.Add("PIX");

                if (ramos.Count > 1)
                    pendencias.Add($"O destino do pagamento do frete aceita apenas uma forma e foram informadas {ramos.Count} ({string.Join(", ", ramos)}).");

                if (ramos.Count == 1 && ramos[0] == "banco/agência")
                {
                    if (string.IsNullOrWhiteSpace(mdfe.PagamentoBanco))
                        pendencias.Add("Informe o código do banco junto com a agência do pagamento do frete.");
                    if (string.IsNullOrWhiteSpace(mdfe.PagamentoAgencia))
                        pendencias.Add("Informe o código da agência junto com o banco do pagamento do frete.");
                }
            }

            // RM10 — documento já vinculado a manifesto ativo.
            var chavesInformadas = (mdfe.Documentos ?? new List<MdfeDocumento>())
                .Select(d => SomenteDigitos(d.ChaveNFe))
                .Where(c => c != null && c.Length == 44)
                .ToList();

            if (chavesInformadas.Count > 0)
            {
                var vinculadas = await _mdfeRepository.GetChavesVinculadasAsync(idCompany, mdfe.Id);
                foreach (var chave in chavesInformadas.Where(c => vinculadas.Contains(c!)))
                    pendencias.Add($"A NF-e de chave {chave} já está vinculada a outro manifesto ativo.");
            }

            // RM11 — peso acima da capacidade somada do comboio: AVISO, não bloqueio.
            var capacidadeTotal = veiculos.Sum(v => v.CapacityKg ?? 0);
            if (capacidadeTotal > 0 && mdfe.PesoBruto > capacidadeTotal)
                avisos.Add($"O peso bruto do manifesto ({mdfe.PesoBruto:N3} KG) excede a capacidade somada do comboio ({capacidadeTotal:N0} KG). O MDF-e não recusa por isso, mas a fiscalização pode autuar.");

            return new MdfeValidacaoResult
            {
                IsReady = pendencias.Count == 0,
                PendingItems = pendencias,
                Warnings = avisos
            };
        }

        /// <summary>
        /// RM14 — coerência entre o tipo de operação e a natureza das notas.
        ///
        /// Só entrada de terceiros → <c>EntradaTerceiros</c>; só saída própria →
        /// <c>SaidaPropria</c>. Documentos das duas naturezas no mesmo manifesto é
        /// operação mista: o manifesto até aceita, mas declarar um tipo só seria
        /// mentir sobre metade da carga, então a escolha tem de ser EXPLÍCITA e
        /// coerente com o que veio.
        /// </summary>
        private async Task ValidarTipoOperacaoAsync(MdfeCreateDto dto, int idCompany, int? idMdfe)
        {
            var documentos = dto.Documentos ?? new List<MdfeDocumentoDto>();
            if (documentos.Count == 0)
                return;

            var temEntrada = documentos.Any(d => d.TipoDocumento == MdfeTipoDocumento.Entrada);
            var temSaida = documentos.Any(d => d.TipoDocumento == MdfeTipoDocumento.Saida);

            if (temEntrada && temSaida)
                return; // misto: a escolha do usuário é a que vale.

            if (temEntrada && dto.TipoOperacao != MdfeTipoOperacao.EntradaTerceiros)
                throw new DomainException("As notas selecionadas são todas de ENTRADA (compra de terceiros), mas o manifesto está como Saída Própria. Ajuste o tipo de operação.");

            if (temSaida && dto.TipoOperacao != MdfeTipoOperacao.SaidaPropria)
                throw new DomainException("As notas selecionadas são todas de SAÍDA (venda própria), mas o manifesto está como Entrada de Terceiros. Ajuste o tipo de operação.");

            // Um documento não pode estar em dois manifestos ativos (RM10) — a
            // checagem é antecipada aqui, na gravação, e de novo na geração do XML:
            // entre as duas pode ter passado outro manifesto.
            var chaves = documentos
                .Select(d => SomenteDigitos(d.ChaveNFe))
                .Where(c => c != null && c.Length == 44)
                .ToList();

            if (chaves.Count == 0)
                return;

            var vinculadas = await _mdfeRepository.GetChavesVinculadasAsync(idCompany, idMdfe);
            var conflito = chaves.FirstOrDefault(c => vinculadas.Contains(c!));

            if (conflito != null)
                throw new DomainException($"A NF-e de chave {conflito} já está vinculada a outro manifesto ativo.");
        }

        // =====================================================================
        // Totais (RM01 alimenta tot.vCarga / qCarga / qNFe)
        // =====================================================================

        /// <summary>
        /// Consolida os totais no manifesto antes de gravar: <c>vCarga</c> é a soma
        /// dos valores e <c>qCarga</c> a soma dos pesos.
        ///
        /// Os totais são calculados dos DOCUMENTOS, e não do que o cliente mandou
        /// no cabeçalho: assim o total do XML é sempre a soma do que está na lista,
        /// e a tela não consegue mostrar um total que o XML não confirma.
        ///
        /// Também é aqui que o município de carregamento é fixado, a partir da
        /// configuração fiscal — ver <see cref="AplicarMunicipioDeCarregamento"/>.
        /// </summary>
        private static void ConsolidarTotais(MdfeEmissao mdfe, FiscalConfiguration config)
        {
            var documentos = mdfe.Documentos?.ToList() ?? new List<MdfeDocumento>();

            mdfe.QuantidadeNFe = documentos.Count;
            mdfe.ValorTotal = documentos.Sum(d => d.ValorTotal);
            mdfe.PesoBruto = documentos.Sum(d => d.PesoBruto);

            AplicarMunicipioDeCarregamento(mdfe, config);
        }

        /// <summary>
        /// Fixa <c>infMunCarrega</c> a partir do endereço do EMITENTE, ignorando o
        /// que a tela mandou.
        ///
        /// O campo nasceu editável e o payload é aceito, mas o valor é descartado:
        /// não existe tabela de municípios do IBGE neste sistema, então enquanto o
        /// código vinha da tela nada no caminho até a SEFAZ conferia o valor
        /// contra a tabela real. O XSD exige apenas <c>[0-9]{7}</c>, o DTO exige
        /// apenas 7 caracteres e <c>MdfeBuilder.ExigirCodigoIbge</c> confere apenas
        /// o comprimento — um código inexistente (o <c>1234567</c> que apareceu num
        /// manifesto de teste) atravessava a pilha inteira e só seria recusado pela
        /// SEFAZ, depois de assinado. A configuração fiscal é o único lugar do
        /// sistema onde o código é informado por quem sabe qual é.
        ///
        /// <b>Chamado também de <see cref="GerarXmlAsync"/></b>, e não só daqui: um
        /// manifesto já gravado com um código ruim continuaria gerando XML errado
        /// até alguém editá-lo e salvar de novo, e <c>GerarXmlAsync</c> é o último
        /// ponto antes da assinatura — o único lugar onde a garantia vale de fato.
        ///
        /// <b>Consequência aceita:</b> na entrada de terceiros, em que a carga é
        /// carregada no pátio do fornecedor, o município declarado passa a ser o da
        /// empresa. Trocar isso por uma heurística (derivar da UF, ou do parceiro)
        /// reintroduziria a classe de erro que esta mudança fecha.
        /// </summary>
        private static void AplicarMunicipioDeCarregamento(MdfeEmissao mdfe, FiscalConfiguration config)
        {
            var endereco = config.Emitente?.EmitenteEndereco;

            var codigo = SomenteDigitos(endereco?.CodigoCidade);

            if (codigo?.Length != 7)
                throw new DomainException(
                    "O código IBGE do município da empresa não está preenchido com 7 dígitos na configuração fiscal. " +
                    "O município de carregamento do manifesto é lido de lá.");

            if (string.IsNullOrWhiteSpace(endereco?.Cidade))
                throw new DomainException(
                    "O município da empresa não está preenchido na configuração fiscal. " +
                    "Ele é o município de carregamento declarado no manifesto.");

            mdfe.CodMunCarregamento = codigo;
            mdfe.MunCarregamento = endereco!.Cidade!.Trim();
        }

        // =====================================================================
        // Mapeamento DTO → entidade
        // =====================================================================

        /// <summary>
        /// Substitui as coleções filhas pelas do DTO.
        ///
        /// Substitui, e não faz merge por Id: o rascunho é reescrito inteiro pela
        /// tela, e mesclar deixaria órfãos os documentos que o usuário removeu —
        /// que continuariam no manifesto e no XML sem aparecer na tela.
        /// </summary>
        private static void AplicarFilhos(MdfeEmissao mdfe, MdfeCreateDto dto)
        {
            mdfe.Documentos = (dto.Documentos ?? new List<MdfeDocumentoDto>())
                .Select(d => new MdfeDocumento
                {
                    TipoDocumento = d.TipoDocumento!.Value,
                    NFeEmissionId = d.NFeEmissionId,
                    PurchaseId = d.PurchaseId,
                    ChaveNFe = SomenteDigitos(d.ChaveNFe) ?? d.ChaveNFe,
                    Serie = d.Serie,
                    Numero = d.Numero,
                    DataEmissao = d.DataEmissao,
                    PartnerName = d.PartnerName,
                    UfOrigem = d.UfOrigem,
                    UfDestino = d.UfDestino,
                    CodMunDescarga = SomenteDigitos(d.CodMunDescarga) ?? d.CodMunDescarga,
                    MunicipioDescarga = (d.MunicipioDescarga ?? string.Empty).Trim(),
                    ValorTotal = d.ValorTotal,
                    ValorMercadoria = d.ValorMercadoria,
                    PesoBruto = d.PesoBruto
                })
                .ToList();

            mdfe.Percurso = (dto.Percurso ?? new List<MdfePercursoDto>())
                .Where(p => p != null)
                .OrderBy(p => p.Ordem)
                .Select((p, indice) => new MdfePercurso
                {
                    UfPercurso = NormalizarUf(p.UfPercurso),
                    // A ordem é reindexada pela posição: o que define o trajeto é a
                    // sequência da tela, e buracos na numeração do cliente não
                    // significam nada no XML.
                    Ordem = indice + 1
                })
                .ToList();

            mdfe.Veiculos = (dto.Veiculos ?? new List<MdfeVeiculoDto>())
                .Where(v => v != null)
                .Select(v => new MdfeVeiculo
                {
                    IdVehicle = v.IdVehicle,
                    Tipo = v.Tipo!.Value
                })
                .ToList();

            mdfe.Condutores = (dto.Condutores ?? new List<MdfeCondutorDto>())
                .Where(c => c != null)
                .Select(c => new MdfeCondutor
                {
                    IdClient = c.IdClient,
                    Nome = (c.Nome ?? string.Empty).Trim(),
                    Cpf = SomenteDigitos(c.Cpf) ?? c.Cpf
                })
                .ToList();
        }

        // =====================================================================
        // Mapeamento entidade → DTO
        // =====================================================================

        private static MdfeListItemDto MapearLista(MdfeEmissao m)
        {
            return new MdfeListItemDto
            {
                Id = m.Id,
                Serie = m.Serie,
                Numero = m.Numero,
                ChaveAcesso = m.ChaveAcesso,
                DataEmissao = m.DataEmissao,
                UfCarregamento = m.UfCarregamento,
                UfDescarregamento = m.UfDescarregamento,
                TipoEmitente = m.TipoEmitente,
                TipoOperacao = m.TipoOperacao,
                StatusMdfe = m.StatusMdfe,
                ValorTotal = m.ValorTotal,
                PesoBruto = m.PesoBruto,
                QuantidadeNFe = m.QuantidadeNFe,
                VeiculoTracaoPlaca = m.VeiculoTracao?.LicensePlate,
                MunCarregamento = m.MunCarregamento,
                CreatedAt = m.CreatedAt,
                // A listagem decide a exclusão por aqui, e não pelo status: o que
                // barra o Delete é o XML assinado existir, não o estado — um Erro
                // por rejeição da SEFAZ também já foi transmitido.
                PodeExcluir = string.IsNullOrEmpty(m.XmlCompleto)
            };
        }

        private async Task<MdfeResponseDto> MapearDetalheAsync(MdfeEmissao m, int idCompany)
        {
            var dto = new MdfeResponseDto
            {
                Id = m.Id,
                IdCompany = m.IdCompany,
                Serie = m.Serie,
                Numero = m.Numero,
                ChaveAcesso = m.ChaveAcesso,
                Protocolo = m.Protocolo,
                DataEmissao = m.DataEmissao,
                UfCarregamento = m.UfCarregamento,
                UfDescarregamento = m.UfDescarregamento,
                TipoEmitente = m.TipoEmitente,
                Modal = m.Modal,
                TipoOperacao = m.TipoOperacao,
                StatusMdfe = m.StatusMdfe,
                Sent = m.Sent,
                TryCount = m.TryCount,
                ErrorMessage = m.ErrorMessage,
                CStat = m.CStat,
                XMotivo = m.XMotivo,
                Recibo = m.Recibo,
                DataAutorizacao = m.DataAutorizacao,
                DataEncerramento = m.DataEncerramento,
                ProtocoloEncerramento = m.ProtocoloEncerramento,
                DataCancelamento = m.DataCancelamento,
                JustificativaCancelamento = m.JustificativaCancelamento,
                SequenciaEvento = m.SequenciaEvento,
                XmlCompleto = m.XmlCompleto,
                ValorTotal = m.ValorTotal,
                PesoBruto = m.PesoBruto,
                QuantidadeNFe = m.QuantidadeNFe,
                CodMunCarregamento = m.CodMunCarregamento,
                MunCarregamento = m.MunCarregamento,
                TipoCarga = m.TipoCarga,
                ProdutoPredominante = m.ProdutoPredominante,
                InfoAdFisco = m.InfoAdFisco,
                InfoComplementar = m.InfoComplementar,
                CodigoCIOT = m.CodigoCIOT,
                ContratanteId = m.ContratanteId,
                ContratanteNome = m.Contratante?.Name,
                IndicadorPagamento = m.IndicadorPagamento,
                PagamentoBanco = m.PagamentoBanco,
                PagamentoAgencia = m.PagamentoAgencia,
                PagamentoCnpjIpef = m.PagamentoCnpjIpef,
                PagamentoChavePix = m.PagamentoChavePix,
                IdVeiculoTracao = m.IdVeiculoTracao,
                VeiculoTracaoPlaca = m.VeiculoTracao?.LicensePlate,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                Documentos = (m.Documentos ?? new List<MdfeDocumento>()).Select(d => new MdfeDocumentoDto
                {
                    Id = d.Id,
                    TipoDocumento = d.TipoDocumento,
                    NFeEmissionId = d.NFeEmissionId,
                    PurchaseId = d.PurchaseId,
                    ChaveNFe = d.ChaveNFe,
                    Serie = d.Serie,
                    Numero = d.Numero,
                    DataEmissao = d.DataEmissao,
                    PartnerName = d.PartnerName,
                    UfOrigem = d.UfOrigem,
                    UfDestino = d.UfDestino,
                    CodMunDescarga = d.CodMunDescarga,
                    MunicipioDescarga = d.MunicipioDescarga,
                    ValorTotal = d.ValorTotal,
                    ValorMercadoria = d.ValorMercadoria,
                    PesoBruto = d.PesoBruto
                }).ToList(),
                Percurso = (m.Percurso ?? new List<MdfePercurso>())
                    .OrderBy(p => p.Ordem)
                    .Select(p => new MdfePercursoDto { Id = p.Id, UfPercurso = p.UfPercurso, Ordem = p.Ordem })
                    .ToList(),
                Veiculos = (m.Veiculos ?? new List<MdfeVeiculo>()).Select(v => new MdfeVeiculoDto
                {
                    Id = v.Id,
                    IdVehicle = v.IdVehicle,
                    Tipo = v.Tipo,
                    LicensePlate = v.Vehicle?.LicensePlate,
                    LicensingState = v.Vehicle?.LicensingState,
                    Tare = v.Vehicle?.Tare,
                    CapacityKg = v.Vehicle?.CapacityKg
                }).ToList(),
                Condutores = (m.Condutores ?? new List<MdfeCondutor>()).Select(c => new MdfeCondutorDto
                {
                    Id = c.Id,
                    IdClient = c.IdClient,
                    Nome = c.Nome,
                    Cpf = c.Cpf
                }).ToList()
            };

            dto.PodeCancelar = PodeCancelar(m);
            dto.PodeEncerrar = m.StatusMdfe == MdfeStatus.Autorizado;
            dto.PodeEditar = PodeEditar(m);
            dto.Validacao = await CalcularValidacaoAsync(m, idCompany);
            return dto;
        }

        /// <summary>
        /// Manifesto autorizado, com protocolo, dentro das 24 horas de
        /// <see cref="MdfeEmissao.DataAutorizacao"/>. É o mesmo teste que
        /// <see cref="CancelarAsync"/> faz antes de enviar o evento — aqui só para
        /// a tela poder desabilitar o botão em vez de mostrar o erro depois.
        ///
        /// Sem <c>DataAutorizacao</c> o manifesto é tratado como cancelável: o que
        /// a tela não pode é impedir um cancelamento legítimo por falta de um dado
        /// que já deveria estar gravado.
        /// </summary>
        private static bool PodeCancelar(MdfeEmissao m)
        {
            if (m.StatusMdfe != MdfeStatus.Autorizado || string.IsNullOrWhiteSpace(m.Protocolo))
                return false;

            return !m.DataAutorizacao.HasValue || DateTime.UtcNow <= m.DataAutorizacao.Value.AddHours(24);
        }

        /// <summary>
        /// Manifesto que pode voltar a rascunho para ser corrigido — a mesma regra
        /// que <see cref="AtualizarAsync"/> aplica antes de aceitar a edição, exposta
        /// no detalhe para a tela desabilitar o botão em vez de mostrar o erro
        /// depois. Espelha <see cref="PodeCancelar"/> nesse arranjo.
        ///
        /// <see cref="MdfeStatus.Rascunho"/>: nunca teve XML.
        /// <see cref="MdfeStatus.Erro"/> sem XML: falha de montagem ou assinatura —
        /// <c>MarcarErroAsync</c> roda antes de <c>SalvarXmlAsync</c>, então nada foi
        /// transmitido.
        /// <see cref="MdfeStatus.Erro"/> com XML e <c>CStat</c>: a SEFAZ respondeu e
        /// recusou; o XML assinado não vale nada e pode ser descartado.
        /// <see cref="MdfeStatus.Erro"/> com XML e <b>sem</b> <c>CStat</c>: não dá
        /// para afirmar que houve recusa. A resposta pode ter se perdido com o
        /// documento já autorizado, e reabrir apagaria <c>ChaveAcesso</c> — o único
        /// registro dele. A tela manda consultar a situação antes.
        /// </summary>
        private static bool PodeEditar(MdfeEmissao m)
        {
            if (m.StatusMdfe == MdfeStatus.Rascunho)
                return true;

            if (m.StatusMdfe != MdfeStatus.Erro)
                return false;

            return string.IsNullOrEmpty(m.XmlCompleto) || m.CStat.HasValue;
        }

        /// <summary>Por que <see cref="PodeEditar"/> disse não — a mensagem que a tela mostra.</summary>
        private static string MotivoNaoPodeEditar(MdfeEmissao m)
        {
            if (m.StatusMdfe == MdfeStatus.Erro)
                return "Este manifesto foi transmitido e a resposta da SEFAZ não chegou. Consulte a situação antes de editá-lo: se ele tiver sido autorizado, corrigir os dados apagaria o registro de um documento válido.";

            if (m.StatusMdfe == MdfeStatus.Validado)
                return "Este manifesto tem XML assinado, aguardando transmissão. Transmita-o antes de alterar os dados.";

            return $"O manifesto está com situação '{m.StatusMdfe}' e não pode mais ser editado.";
        }

        private static DocumentoElegivelDto MapearSaida(NFeEmission nota, HashSet<string> chavesVinculadas)
        {
            var cliente = nota.Sale?.Client;
            var peso = (nota.Sale?.SaleItems ?? new List<SaleItems>())
                .Sum(i => (i.Product?.PesoUnitario ?? 0m) * i.Amount);

            var dto = new DocumentoElegivelDto
            {
                Id = nota.Id,
                TipoDocumento = MdfeTipoDocumento.Saida,
                ChaveNFe = nota.ChaveAcesso ?? string.Empty,
                Serie = nota.Serie,
                Numero = nota.Numero,
                DataEmissao = nota.CreatedAt,
                PartnerName = cliente?.Name,
                UfOrigem = null, // a origem da saída é a empresa; ver Impedimentos abaixo
                UfDestino = cliente?.Uf,
                ValorTotal = nota.Sale?.Total ?? 0m,
                ValorMercadoria = nota.Sale?.ValueSale ?? 0m,
                PesoBruto = peso,
                CodMunDescargaSugerido = ClienteCodigoIbge(cliente),
                MunicipioDescarga = cliente?.Municipio
            };

            if (chavesVinculadas.Contains(SomenteDigitos(nota.ChaveAcesso) ?? string.Empty))
                dto.Impedimentos.Add("Esta NF-e já está vinculada a outro manifesto ativo.");

            return dto;
        }

        private static DocumentoElegivelDto MapearEntrada(Purchase compra, HashSet<string> chavesVinculadas)
        {
            var chave = SomenteDigitos(compra.ChaveNfe) ?? string.Empty;

            // Série e número saem DA CHAVE: a Purchase não guarda esses campos
            // próprios, porque a importação do XML de compra só preserva a chave.
            // Posições 22-24 (série) e 25-33 (número) do layout da chave.
            string? serie = null;
            long? numero = null;
            if (chave.Length == 44)
            {
                serie = chave.Substring(22, 3);
                if (long.TryParse(chave.Substring(25, 9), out var n))
                    numero = n;
            }

            var peso = (compra.PurchaseItems ?? new List<PurchaseItem>())
                .Sum(i => (i.Produto?.PesoUnitario ?? 0m) * i.Quantidade);

            var dto = new DocumentoElegivelDto
            {
                Id = compra.Id,
                TipoDocumento = MdfeTipoDocumento.Entrada,
                ChaveNFe = chave,
                Serie = serie,
                Numero = numero,
                DataEmissao = compra.DataEntrada,
                PartnerName = compra.Fornecedor?.nome ?? compra.NomeFornecedor,
                UfOrigem = compra.Fornecedor?.uf,
                UfDestino = null, // o destino da entrada é a empresa
                ValorTotal = compra.ValorNotaFiscal ?? compra.ValorTotal,
                ValorMercadoria = compra.ValorProdutos ?? compra.ValorTotal,
                PesoBruto = peso
            };

            if (chave.Length != 44)
                dto.Impedimentos.Add("A nota de entrada está sem chave de acesso de 44 dígitos.");

            if (chavesVinculadas.Contains(chave))
                dto.Impedimentos.Add("Esta NF-e já está vinculada a outro manifesto ativo.");

            return dto;
        }

        // =====================================================================
        // Configuração fiscal e certificado
        // =====================================================================

        private async Task<FiscalConfiguration> ObterConfiguracaoFiscalAsync(int idCompany)
        {
            var config = await _fiscalRepository.GetByCompany(idCompany);
            if (config == null)
                throw new DomainException("A empresa não tem configuração fiscal cadastrada — sem ela não há emitente nem numeração de MDF-e.");

            return config;
        }

        /// <summary>
        /// Monta a <see cref="MDFeConfiguracao"/> DESTA emissão.
        ///
        /// <b>Nunca usar <c>MDFeConfiguracao.Instancia</c>.</b> O singleton estático
        /// é o padrão de todos os métodos da biblioteca quando não se passa uma
        /// configuração, e é compartilhado entre requisições concorrentes — duas
        /// emissões simultâneas de empresas diferentes escreveriam uma na
        /// configuração da outra. Aqui a configuração é criada por chamada e
        /// descartada no fim.
        ///
        /// <c>VersaoWebService.VersaoLayout</c> é o campo que <c>Assina()</c> grava
        /// no <c>versao</c> do XML e que <c>Valida()</c> usa para escolher o XSD —
        /// deixá-lo no padrão faria o manifesto sair como v1.00 e ser validado
        /// contra o schema errado.
        /// </summary>
        private MDFeConfiguracao MontarConfiguracaoMdfe(FiscalConfiguration config)
        {
            var certificado = config.CertificadoDigital
                              ?? throw new DomainException("A empresa não tem certificado digital configurado — sem ele o manifesto não pode ser assinado.");

            if (string.IsNullOrWhiteSpace(certificado.Arquivo))
                throw new DomainException("O certificado digital da empresa está sem arquivo informado.");

            var nomeArquivo = Path.GetFileName(certificado.Arquivo);

            var caminhoCertificado = Environment.GetEnvironmentVariable("RENDER") == "true"
                ? Path.Combine("/app/wwwroot/certs", nomeArquivo)
                : Path.Combine(_environment.WebRootPath, "certs", nomeArquivo);

            // System.IO.File qualificado: o Model tem um tipo File próprio
            // (Model.Registrations.File), e o using de Model torna "File" ambíguo.
            if (!System.IO.File.Exists(caminhoCertificado))
                throw new DomainException($"O arquivo do certificado digital não foi encontrado em '{caminhoCertificado}'.");

            var configuracaoCertificado = new ConfiguracaoCertificado
            {
                TipoCertificado = DFe.Utils.TipoCertificado.A1Arquivo,
                Arquivo = caminhoCertificado,
                Senha = certificado.Senha,
                ManterDadosEmCache = false,
                KeyStorageFlags =
                    X509KeyStorageFlags.UserKeySet |
                    X509KeyStorageFlags.PersistKeySet |
                    X509KeyStorageFlags.Exportable
            };

            var diretorioSchemas = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NFSchemas");

            return new MDFeConfiguracao
            {
                ConfiguracaoCertificado = configuracaoCertificado,
                CaminhoSchemas = diretorioSchemas,
                IsAdicionaQrCode = true,
                VersaoWebService = new MDFeVersaoWebService
                {
                    // Da constante do builder, não de um literal: "VersaoServico" é
                    // ambíguo entre DFe.Classes.Flags e MDFe.Utils.Flags (os dois
                    // namespaces estão em escopo), e o builder é quem fixa o leiaute.
                    VersaoLayout = MdfeBuilder.VersaoLayout,
                    UfEmitente = (Estado)Enum.Parse(typeof(Estado), NormalizarUf(config.Emitente?.EmitenteEndereco?.Uf ?? string.Empty)),
                    TipoAmbiente = config.Ambiente == AmbienteEnum.Homologacao ? TipoAmbiente.Homologacao : TipoAmbiente.Producao
                }
            };
        }

        // =====================================================================
        // Auxiliares
        // =====================================================================

        /// <summary>
        /// A mensagem de um erro de validação de schema é a informação mais útil
        /// que existe para consertar o XML, então ela é preservada inteira. O
        /// validador da biblioteca devolve só o primeiro erro, então a mensagem
        /// costuma vir uma por vez — corrigir e gerar de novo é o fluxo.
        /// </summary>
        private static string MensagemDeValidacao(Exception ex)
        {
            var mensagem = ex.Message;

            if (ex.InnerException != null && !string.IsNullOrWhiteSpace(ex.InnerException.Message))
                mensagem += " Detalhe: " + ex.InnerException.Message;

            return mensagem.Length > 1000 ? mensagem.Substring(0, 1000) : mensagem;
        }

        private static string NormalizarUf(string? uf)
        {
            return (uf ?? string.Empty).Trim().ToUpperInvariant();
        }

        /// <summary>
        /// Série do manifesto, canonicamente com 3 dígitos.
        ///
        /// A canonicalização não é cosmética: a numeração (RM09) acha o último
        /// número comparando a série por IGUALDADE DE STRING, então "1" e "001"
        /// seriam duas séries distintas e cada uma começaria do 1 — dois
        /// manifestos com o mesmo número na mesma série. Fixar em 3 dígitos (que
        /// é o que o leiaute usa, 0..999) elimina a ambiguidade na origem.
        ///
        /// A validação vem ANTES da gravação de propósito: a coluna é
        /// varchar(3), então uma série fora do leiaute estouraria no Postgres com
        /// erro de tamanho, e não com uma mensagem que o usuário entenda.
        /// </summary>
        private static string NormalizarSerie(string? serie)
        {
            var texto = (serie ?? string.Empty).Trim();
            if (texto.Length == 0)
                throw new DomainException("A série do MDF-e não está configurada em FiscalConfiguration.NumeracaoDocumentos.Mdfe.");

            if (!int.TryParse(texto, out var numero) || numero < 0 || numero > 999)
                throw new DomainException($"A série do MDF-e deve ser numérica, entre 0 e 999 (configurada: '{serie}').");

            return numero.ToString("000");
        }

        private static string? Limpar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private static string? SomenteDigitos(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            var digitos = new string(valor.Where(char.IsDigit).ToArray());
            return digitos.Length == 0 ? null : digitos;
        }

        /// <summary>
        /// Código IBGE do município do parceiro. Sai do endereço, e não de uma
        /// tabela: o sistema não tem tabela de municípios — o código e o nome
        /// moram no cadastro do parceiro (<c>CodMunicipioIbge</c> e
        /// <c>Municipio</c>).
        /// </summary>
        private static string? ClienteCodigoIbge(Client? cliente)
        {
            if (cliente == null)
                return null;

            var codigo = SomenteDigitos(cliente.CodMunicipioIbge);
            return codigo?.Length == 7 ? codigo : null;
        }
    }

    /// <summary>
    /// Contrato do serviço de MDF-e. Interface no fim do mesmo arquivo, como
    /// <c>VehicleService</c> e <c>MdfeRepository</c> fazem.
    /// </summary>
    public interface IMdfeService : IBaseService<MdfeEmissao>
    {
        Task<PagedResult<MdfeListItemDto>> GetPagedAsync(Filters filter);
        Task<MdfeResponseDto> GetByIdAsync(int id, int idCompany);
        Task<MdfeProximoNumeroDto> GetProximoNumeroAsync(int idCompany);
        Task<List<DocumentoElegivelDto>> PesquisarDocumentosAsync(Filters filter, int idCompany);
        Task<MdfeResponseDto> CriarAsync(MdfeCreateDto dto, int idCompany);
        Task<MdfeResponseDto> AtualizarAsync(int id, MdfeUpdateDto dto, int idCompany);
        Task DeletarAsync(int id, int idCompany);
        Task<MdfeValidacaoResult> ValidarAsync(int id, int idCompany);
        Task<MdfeResponseDto> GerarXmlAsync(int id, int idCompany);
        Task<(byte[] Bytes, string NomeArquivo)> ObterXmlAsync(int id, int idCompany);

        /// <summary>Transmite o XML assinado à SEFAZ e grava o protocolo ou a rejeição.</summary>
        Task<MdfeResponseDto> TransmitirAsync(int id, int idCompany);

        /// <summary>Situação do manifesto na SEFAZ pela chave — sincroniza o estado local.</summary>
        Task<MdfeResponseDto> ConsultarSituacaoAsync(int id, int idCompany);

        /// <summary>Situação do serviço do MDF-e na SEFAZ (<c>consStatServMDFe</c>).</summary>
        Task<MdfeStatusServicoDto> ConsultarStatusServicoAsync(int idCompany);

        /// <summary>Encerra o manifesto autorizado (<c>evEncMDFe</c>).</summary>
        Task<MdfeResponseDto> EncerrarAsync(int id, int idCompany, MdfeEncerrarDto dto);

        /// <summary>Cancela o manifesto autorizado, dentro de 24h (<c>evCancMDFe</c>).</summary>
        Task<MdfeResponseDto> CancelarAsync(int id, int idCompany, MdfeCancelarDto dto);

        /// <summary>DAMDFe em PDF do manifesto autorizado.</summary>
        Task<(byte[] Bytes, string NomeArquivo)> ObterDamdfeAsync(int id, int idCompany);
    }
}