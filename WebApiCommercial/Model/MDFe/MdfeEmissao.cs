using Model.Enums;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.MDFe
{
    /// <summary>
    /// Manifesto Eletrônico de Documentos Fiscais (modelo 58) — o documento que
    /// autoriza a circulação da carga e agrupa as NF-e transportadas.
    ///
    /// O registro guarda tudo que é preciso para MONTAR, ASSINAR e VALIDAR o XML
    /// contra os XSDs do leiaute <c>mdfe_v3.00</c>, e também o ciclo de vida
    /// depois disso: transmissão, consulta, encerramento e cancelamento.
    ///
    /// O <see cref="XmlCompleto"/> guarda o XML <b>assinado</b> enquanto o
    /// manifesto não foi autorizado, e passa a guardar o <c>procMDFe</c>
    /// (manifesto + protocolo) depois — mesmo arranjo que o <c>NFeEmission</c>
    /// usa com o <c>nfeProc</c>. O <c>MDFe</c> continua dentro, então nada se
    /// perde, e os eventos e a DAMDFe (que precisam do protocolo) leem de um
    /// lugar só.
    ///
    /// A tabela é a que faltava para <c>VehicleUsageHistory.SourceId</c>, que
    /// estava documentado como "nulo para MDF-e até que a tabela de manifesto
    /// seja criada" — ver <c>MdfeService</c>, que passa a preenchê-lo.
    ///
    /// Multi-tenant: <see cref="IdCompany"/> é filtrado EXPLICITAMENTE em cada
    /// método do <c>MdfeRepository</c> (o projeto não tem <c>HasQueryFilter</c>).
    /// </summary>
    public class MdfeEmissao : BaseEntity
    {
        /// <summary>Empresa dona do manifesto (multi-tenant).</summary>
        public int IdCompany { get; set; }
        public Company Company { get; set; }

        /// <summary>
        /// Série e número vêm de
        /// <c>FiscalConfiguration.NumeracaoDocumentos.Mdfe</c> (RM09), e não de um
        /// contador próprio desta tabela — é o mesmo lugar onde já moram a série
        /// da NF-e, da NFC-e e da DPS. O número é reservado em transação, junto
        /// com o registro, para dois pedidos simultâneos não tirarem o mesmo
        /// (o <c>NFeService</c> faz read-modify-write sem transação e tem esse
        /// defeito — não repetir aqui).
        /// </summary>
        [Required]
        [StringLength(3)]
        public string Serie { get; set; }

        public long Numero { get; set; }

        /// <summary>
        /// Chave de acesso de 44 dígitos (modelo 58), gerada por
        /// <c>DFe.Utils.ChaveFiscal.ObterChave</c>. Fica nula enquanto o manifesto
        /// é rascunho — é preenchida quando o XML é gerado.
        /// </summary>
        [StringLength(44)]
        public string? ChaveAcesso { get; set; }

        /// <summary>Número do protocolo de autorização. Vazio na fase 1 (sem transmissão).</summary>
        [StringLength(20)]
        public string? Protocolo { get; set; }

        public DateTime DataEmissao { get; set; }

        /// <summary>UF onde a carga é carregada (<c>infMDFe.ide.UFIni</c>). RM12 valida contra <see cref="UfList"/>.</summary>
        [Required]
        [StringLength(2)]
        public string UfCarregamento { get; set; }

        /// <summary>
        /// UF de destino da carga (<c>infMDFe.ide.UFFim</c>).
        ///
        /// Atenção: o MDF-e agrupa os documentos por MUNICÍPIO de descarga
        /// (<c>infMunDescarga</c>), não por UF — a UF aqui é só o destino final
        /// declarado, e a coerência com os municípios dos documentos é a RM04.
        /// </summary>
        [Required]
        [StringLength(2)]
        public string UfDescarregamento { get; set; }

        /// <summary>
        /// PST ou Carga Própria, escolhido na tela. Muda o conjunto de regras
        /// aplicado (RM08) — ver <see cref="MdfeTipoEmitente"/>.
        /// </summary>
        [Required]
        public MdfeTipoEmitente TipoEmitente { get; set; }

        /// <summary>Modal de transporte. Só <see cref="MdfeModal.Rodoviario"/> é aceito na fase 1.</summary>
        [Required]
        public MdfeModal Modal { get; set; } = MdfeModal.Rodoviario;

        /// <summary>
        /// Entrada de terceiros ou saída própria (RM14). Derivado das notas
        /// selecionadas; o serviço exige escolha explícita quando as duas
        /// naturezas aparecem juntas.
        /// </summary>
        [Required]
        public MdfeTipoOperacao TipoOperacao { get; set; }

        public MdfeStatus StatusMdfe { get; set; } = MdfeStatus.Rascunho;

        /// <summary>true quando transmitido e autorizado.</summary>
        public bool Sent { get; set; }

        /// <summary>Tentativas de transmissão, incrementadas a cada envio.</summary>
        public int TryCount { get; set; }

        /// <summary>
        /// Motivo da falha quando <see cref="StatusMdfe"/> é
        /// <see cref="MdfeStatus.Erro"/>.
        ///
        /// Cobre a falha de MONTAGEM/ASSINATURA — o XML que não passou no
        /// validador. A recusa da SEFAZ vai em <see cref="XMotivo"/>: são coisas
        /// diferentes, e misturá-las apagaria a diferença entre "o XML está
        /// errado" (consertável aqui) e "a SEFAZ recusou" (só se lê o motivo
        /// dela).
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// XML <b>assinado</b> (enquanto não autorizado) e depois <c>procMDFe</c>,
        /// com o protocolo. Coluna <c>text</c>: o XML passa de 8 KB com
        /// facilidade (um manifesto por município de descarga).
        /// </summary>
        public string? XmlCompleto { get; set; }

        /// <summary>
        /// Resposta crua da SEFAZ, em JSON. É o que permite investigar uma
        /// rejeição depois do fato — sem ela, o que sobra é o
        /// <see cref="XMotivo"/>, que é uma linha.
        /// </summary>
        public string? ResponseJson { get; set; }

        // ------------------------------------------------------------------
        // Retorno da transmissão (o que a SEFAZ devolveu)
        // ------------------------------------------------------------------

        /// <summary>
        /// Código de situação do retorno (<c>cStat</c>): 100 é autorizado. É o
        /// campo que decide autorizado × rejeitado — sem ele não dá para
        /// distinguir "recusado" de "nem tentou".
        /// </summary>
        public int? CStat { get; set; }

        /// <summary>Motivo literal da SEFAZ (<c>xMotivo</c>).</summary>
        [StringLength(255)]
        public string? XMotivo { get; set; }

        /// <summary>
        /// Recibo do lote. No fluxo síncrono do MDF-e costuma vir nulo, mas é o
        /// que permite reprocessar quando a SEFAZ cai no meio do caminho.
        /// </summary>
        [StringLength(20)]
        public string? Recibo { get; set; }

        /// <summary>
        /// Data/hora da autorização. É a base da janela de 24h do cancelamento —
        /// sem ela não há como conferir o prazo sem perguntar à SEFAZ.
        /// </summary>
        public DateTime? DataAutorizacao { get; set; }

        // ------------------------------------------------------------------
        // Eventos (encerramento e cancelamento)
        // ------------------------------------------------------------------

        /// <summary>Data/hora do encerramento. O encerramento também tem prazo.</summary>
        public DateTime? DataEncerramento { get; set; }

        /// <summary>
        /// Protocolo do evento de encerramento — distinto de
        /// <see cref="Protocolo"/>, que é o da autorização do manifesto.
        /// </summary>
        [StringLength(20)]
        public string? ProtocoloEncerramento { get; set; }

        /// <summary>Data/hora do cancelamento.</summary>
        public DateTime? DataCancelamento { get; set; }

        /// <summary>
        /// Justificativa enviada no cancelamento. A SEFAZ exige de 15 a 255
        /// caracteres; guardar o que foi enviado é o que permite responder
        /// depois o que foi alegado.
        /// </summary>
        [StringLength(255)]
        public string? JustificativaCancelamento { get; set; }

        /// <summary>
        /// Sequência do último evento aceito. A SEFAZ exige 1, 2, 3… por
        /// manifesto e recusa duplicidade — sem este contador, o segundo evento
        /// do mesmo manifesto seria rejeitado.
        /// </summary>
        public int SequenciaEvento { get; set; }

        /// <summary>Soma dos valores totais das NF-e vinculadas (<c>tot.vCarga</c>).</summary>
        public decimal ValorTotal { get; set; }

        // ------------------------------------------------------------------
        // Grupo ide.infMunCarrega e prodPred — aba "Carga/Produtos".
        // ------------------------------------------------------------------

        /// <summary>
        /// Código IBGE do município onde a carga é CARREGADA
        /// (<c>ide.infMunCarrega.cMunCarrega</c>), com 7 dígitos.
        ///
        /// <b>Não vem da tela.</b> É sempre o município do endereço do emitente em
        /// <c>FiscalConfiguration.Emitente.EmitenteEndereco.CodigoCidade</c>, e
        /// <c>MdfeService.AplicarMunicipioDeCarregamento</c> sobrescreve o que vier
        /// no DTO — na criação, na edição e na regeração do XML.
        ///
        /// O motivo é que o código IBGE não existe em nenhum outro lugar do
        /// sistema: não há tabela de municípios aqui, e enquanto o campo era texto
        /// livre nada entre o formulário e o XSD conferia o valor contra a tabela
        /// do IBGE. O XSD exige só <c>[0-9]{7}</c>, o DTO exige só 7 caracteres, e
        /// <c>MdfeBuilder.ExigirCodigoIbge</c> só confere o comprimento — então um
        /// código inexistente (o <c>1234567</c> que apareceu num manifesto de
        /// teste) atravessava a pilha inteira e só seria recusado pela SEFAZ, já
        /// com o manifesto assinado. O cadastro fiscal é o único lugar onde o
        /// código é informado por quem sabe qual é.
        ///
        /// <b>Consequência conhecida:</b> na entrada de terceiros, em que a carga é
        /// carregada no pátio do fornecedor, o município declarado passa a ser o da
        /// empresa e não o do fornecedor. É uma escolha deliberada — trocar a
        /// origem do dado por uma heurística reintroduziria o problema que esta
        /// mudança fecha.
        /// </summary>
        [Required]
        [StringLength(7)]
        public string CodMunCarregamento { get; set; }

        /// <summary>
        /// Nome do município de carregamento (<c>xMunCarrega</c>, 2 a 60
        /// caracteres). Acompanha <see cref="CodMunCarregamento"/> e vem da mesma
        /// origem — <c>Emitente.EmitenteEndereco.Cidade</c>.
        /// </summary>
        [Required]
        [StringLength(60)]
        public string MunCarregamento { get; set; }

        /// <summary>Tipo de carga predominante (<c>prodPred.tpCarga</c>) — aba Carga/Produtos.</summary>
        [Required]
        public MdfeTipoCarga TipoCarga { get; set; } = MdfeTipoCarga.CargaGeral;

        /// <summary>
        /// Descrição do produto predominante (<c>prodPred.xProd</c>, até 120
        /// caracteres). O serviço sugere o item de maior valor entre as notas.
        /// </summary>
        [Required]
        [StringLength(120)]
        public string ProdutoPredominante { get; set; }

        // ------------------------------------------------------------------
        // Grupo infAdic — aba "Informações Adicionais".
        // ------------------------------------------------------------------

        /// <summary>Informação adicional de interesse do fisco (<c>infAdic.infAdFisco</c>).</summary>
        [StringLength(2000)]
        public string? InfoAdFisco { get; set; }

        /// <summary>Informação complementar de interesse do contribuinte (<c>infAdic.infCpl</c>).</summary>
        [StringLength(5000)]
        public string? InfoComplementar { get; set; }

        /// <summary>Soma dos pesos brutos (<c>tot.pesoBruto</c>).</summary>
        public decimal PesoBruto { get; set; }

        /// <summary>Quantidade de NF-e no manifesto (<c>tot.qNFe</c>).</summary>
        public int QuantidadeNFe { get; set; }

        // ------------------------------------------------------------------
        // Grupo infANTT — só para PST (tpEmit = 1), conforme RM08.
        // ------------------------------------------------------------------

        /// <summary>
        /// Código CIOT da operação de transporte (<c>infANTT.infCIOT.CIOT</c>).
        /// Só se aplica a PST; nulo em carga própria.
        /// </summary>
        [StringLength(12)]
        public string? CodigoCIOT { get; set; }

        /// <summary>
        /// Contratante do serviço de transporte (<c>infANTT.infContratante</c>).
        /// Aponta para o PARCEIRO (<see cref="Client"/>), que já tem CPF/CNPJ e UF.
        /// Só se aplica a PST.
        /// </summary>
        public int? ContratanteId { get; set; }
        public Client? Contratante { get; set; }

        /// <summary>
        /// Forma de pagamento do frete (<c>infPag.indPag</c>) — aba "Dados de
        /// Pagamento". Só entra no XML quando <see cref="TipoEmitente"/> é PST:
        /// em carga própria não há contrato de frete a declarar, e o grupo
        /// <c>infANTT</c> inteiro é omitido.
        ///
        /// Só produz efeito junto com um destino de pagamento (ver
        /// <see cref="PagamentoBanco"/>): dentro de <c>infPag</c> o grupo
        /// <c>infBanc</c> é OBRIGATÓRIO, então sem destino o grupo inteiro é
        /// omitido em vez de sair incompleto.
        /// </summary>
        public MdfeIndicadorPagamento IndicadorPagamento { get; set; } = MdfeIndicadorPagamento.AVista;

        // ------------------------------------------------------------------
        // infANTT.infPag.infBanc — destino do pagamento do frete.
        //
        // No leiaute, infBanc é um xs:choice de TRÊS alternativas mutuamente
        // exclusivas: (codBanco + codAgencia) | CNPJIPEF | PIX. Não há
        // discriminador aqui de propósito: os três pares moram em colunas
        // próprias e o MdfeBuilder decide qual ramo está completo — e RECUSA
        // quando mais de um está, porque um discriminador poderia discordar dos
        // valores preenchidos e escolher o ramo errado em silêncio.
        // ------------------------------------------------------------------

        /// <summary>Número do banco (<c>infBanc.codBanco</c>, 3 a 5 dígitos). Par obrigatório com <see cref="PagamentoAgencia"/>.</summary>
        [StringLength(5)]
        public string? PagamentoBanco { get; set; }

        /// <summary>Número da agência (<c>infBanc.codAgencia</c>, 1 a 10 caracteres). Par obrigatório com <see cref="PagamentoBanco"/>.</summary>
        [StringLength(10)]
        public string? PagamentoAgencia { get; set; }

        /// <summary>CNPJ da instituição de pagamento eletrônico do frete (<c>infBanc.CNPJIPEF</c>).</summary>
        [StringLength(14)]
        public string? PagamentoCnpjIpef { get; set; }

        /// <summary>Chave PIX para recebimento do frete (<c>infBanc.PIX</c>, 2 a 60 caracteres).</summary>
        [StringLength(60)]
        public string? PagamentoChavePix { get; set; }

        // ------------------------------------------------------------------
        // Composição
        // ------------------------------------------------------------------

        /// <summary>
        /// Veículo de tração escalado. Guardado também como FK direta (além da
        /// coleção) porque é ÚNICO no manifesto e a tela precisa dele em toda
        /// requisição — buscar via coleção seria um join a mais para o caso mais
        /// comum. RM05 exige que esteja apto (<c>VehicleService.CalculateMdfEStatus</c>).
        /// </summary>
        public int? IdVeiculoTracao { get; set; }
        public Vehicle? VeiculoTracao { get; set; }

        /// <summary>NF-e manifestadas, agrupadas por município de descarga no XML (RM03).</summary>
        public ICollection<MdfeDocumento> Documentos { get; set; } = new List<MdfeDocumento>();

        /// <summary>UFs de percurso (<c>infMDFe.infPercurso</c>) — RM13.</summary>
        public ICollection<MdfePercurso> Percurso { get; set; } = new List<MdfePercurso>();

        /// <summary>Veículos do comboio: 1 tração + 0..5 reboques.</summary>
        public ICollection<MdfeVeiculo> Veiculos { get; set; } = new List<MdfeVeiculo>();

        /// <summary>Condutores (<c>infModal.rodo.infCIOT</c>...) — RM06 exige ao menos um válido.</summary>
        public ICollection<MdfeCondutor> Condutores { get; set; } = new List<MdfeCondutor>();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
