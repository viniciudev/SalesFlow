using Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    /// <summary>
    /// Payload de criação/atualização do manifesto (rascunho).
    ///
    /// A validação é em duas camadas, de propósito, como em
    /// <see cref="VehicleCreateDto"/>:
    /// <list type="bullet">
    ///   <item>DataAnnotations aqui — ausência, tamanho e faixa. O
    ///   <c>[ApiController]</c> converte em 400 sozinho.</item>
    ///   <item>Regras RM cruzadas no <c>MdfeService</c> — coerência entre notas e
    ///   operação (RM14), aptidão do veículo (RM05), RNTRC do emitente (RM08),
    ///   documento já manifestado (RM10). Essas dependem do banco ou de outra
    ///   entidade e não cabem em atributo.</item>
    /// </list>
    ///
    /// Os enums são ANULÁVEIS porque <c>MdfeTipoEmitente.Pst</c> é 1 e
    /// <c>MdfeStatus.Rascunho</c> também: com tipo não anulável, "não informado"
    /// e "PST" ficariam indistinguíveis. Nulo + <c>[Required]</c> separa os dois.
    ///
    /// Série e número NÃO estão aqui: são atribuídos pelo serviço a partir de
    /// <c>FiscalConfiguration.NumeracaoDocumentos.Mdfe</c> (RM09). Aceitar um
    /// número do cliente e ignorá-lo seria pior do que não aceitar — a tela
    /// mostra o próximo número por <c>GET /api/Mdfe/proximo-numero</c>.
    /// </summary>
    public class MdfeCreateDto
    {
        [Required(ErrorMessage = "A data de emissão é obrigatória.")]
        public DateTime DataEmissao { get; set; }

        [Required(ErrorMessage = "O tipo de emitente é obrigatório.")]
        public MdfeTipoEmitente? TipoEmitente { get; set; }

        [Required(ErrorMessage = "O modal é obrigatório.")]
        public MdfeModal? Modal { get; set; }

        [Required(ErrorMessage = "O tipo de operação é obrigatório.")]
        public MdfeTipoOperacao? TipoOperacao { get; set; }

        [Required(ErrorMessage = "A UF de carregamento é obrigatória.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "A UF deve ter 2 letras.")]
        public string UfCarregamento { get; set; } = string.Empty;

        [Required(ErrorMessage = "A UF de descarregamento é obrigatória.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "A UF deve ter 2 letras.")]
        public string UfDescarregamento { get; set; } = string.Empty;

        // --- Aba "Carga/Produtos" ---
        // O município de CARREGAMENTO é obrigatório no leiaute e não é derivável
        // da UF: na entrada de terceiros ele é o do fornecedor, não o da empresa.
        // A tela sugere um valor (ver MdfeService) e o usuário confirma.

        [Required(ErrorMessage = "O município de carregamento é obrigatório.")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "O código IBGE do município deve ter 7 dígitos.")]
        public string CodMunCarregamento { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome do município de carregamento é obrigatório.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "O nome do município deve ter entre 2 e 60 caracteres.")]
        public string MunCarregamento { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo de carga é obrigatório.")]
        public MdfeTipoCarga? TipoCarga { get; set; }

        [Required(ErrorMessage = "O produto predominante é obrigatório.")]
        [StringLength(120, MinimumLength = 1, ErrorMessage = "O produto predominante deve ter entre 1 e 120 caracteres.")]
        public string ProdutoPredominante { get; set; } = string.Empty;

        // --- Aba "Informações Adicionais" (opcionais no leiaute) ---

        [StringLength(2000, ErrorMessage = "A informação ao fisco deve ter no máximo 2000 caracteres.")]
        public string? InfoAdFisco { get; set; }

        [StringLength(5000, ErrorMessage = "A informação complementar deve ter no máximo 5000 caracteres.")]
        public string? InfoComplementar { get; set; }

        // --- Aba "Dados de Pagamento" ---
        // Só produz efeito quando TipoEmitente = PST (RM08); em carga própria o
        // grupo infANTT inteiro é omitido, e estes campos são ignorados.
        //
        // As três formas de destino são MUTUAMENTE EXCLUSIVAS (o leiaute define
        // infBanc como xs:choice) e nenhuma é obrigatória: sem destino, o grupo
        // infPag é omitido — o que é válido. Preencher duas é erro do serviço.

        public MdfeIndicadorPagamento? IndicadorPagamento { get; set; }

        /// <summary>Banco do frete (<c>infBanc.codBanco</c>). Par com <see cref="PagamentoAgencia"/>.</summary>
        [StringLength(5, ErrorMessage = "O código do banco deve ter no máximo 5 caracteres.")]
        public string? PagamentoBanco { get; set; }

        /// <summary>Agência do frete (<c>infBanc.codAgencia</c>). Par com <see cref="PagamentoBanco"/>.</summary>
        [StringLength(10, ErrorMessage = "O código da agência deve ter no máximo 10 caracteres.")]
        public string? PagamentoAgencia { get; set; }

        /// <summary>CNPJ da instituição de pagamento eletrônico do frete (<c>infBanc.CNPJIPEF</c>).</summary>
        [StringLength(14, MinimumLength = 14, ErrorMessage = "O CNPJ da instituição de pagamento deve ter 14 dígitos.")]
        public string? PagamentoCnpjIpef { get; set; }

        /// <summary>Chave PIX para recebimento do frete (<c>infBanc.PIX</c>).</summary>
        [StringLength(60, ErrorMessage = "A chave PIX deve ter no máximo 60 caracteres.")]
        public string? PagamentoChavePix { get; set; }

        /// <summary>
        /// Veículo de tração (RM05). Obrigatório apenas no modal rodoviário —
        /// por isso a checagem é no serviço, e não com <c>[Required]</c>.
        /// </summary>
        public int? IdVeiculoTracao { get; set; }

        /// <summary>
        /// CIOT, exigido quando <see cref="TipoEmitente"/> é PST (RM08). Vai em
        /// <c>infANTT.infCIOT.CIOT</c>.
        /// </summary>
        [StringLength(12, ErrorMessage = "O CIOT deve ter no máximo 12 caracteres.")]
        public string? CodigoCIOT { get; set; }

        /// <summary>Contratante do transporte, exigido quando PST (RM08).</summary>
        public int? ContratanteId { get; set; }

        /// <summary>NF-e manifestadas (RM01 exige ao menos uma).</summary>
        public List<MdfeDocumentoDto> Documentos { get; set; } = new();

        /// <summary>UFs de percurso, em ordem (RM13).</summary>
        public List<MdfePercursoDto> Percurso { get; set; } = new();

        /// <summary>Comboio: 1 tração + 0..5 reboques (RM05).</summary>
        public List<MdfeVeiculoDto> Veiculos { get; set; } = new();

        /// <summary>Condutores (RM06 exige ao menos um).</summary>
        public List<MdfeCondutorDto> Condutores { get; set; } = new();
    }

    /// <summary>
    /// Atualização de rascunho. Mesmas regras; o manifesto só é editável
    /// enquanto não tem XML gerado — depois disso o serviço recusa, porque o XML
    /// está assinado e mudar a origem invalidaria a assinatura.
    /// </summary>
    public class MdfeUpdateDto : MdfeCreateDto
    {
    }

    /// <summary>
    /// Uma NF-e vinculada ao manifesto. Os campos de exibição são aceitos do
    /// cliente porque é a TELA que escolhe a nota na área de pesquisa e devolve o
    /// que mostrou — o serviço revalida a chave e a elegibilidade (RM02/RM10),
    /// mas não vai ao banco reler valor e peso de cada item para conferir o que o
    /// cliente mandou.
    /// </summary>
    public class MdfeDocumentoDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "O tipo do documento é obrigatório.")]
        public MdfeTipoDocumento? TipoDocumento { get; set; }

        /// <summary>Nota de saída (<c>NFeEmission</c>). Obrigatória quando o tipo é Saída.</summary>
        public int? NFeEmissionId { get; set; }

        /// <summary>Nota de entrada (<c>Purchase</c>). Obrigatória quando o tipo é Entrada.</summary>
        public int? PurchaseId { get; set; }

        [Required(ErrorMessage = "A chave da NF-e é obrigatória.")]
        [StringLength(44, MinimumLength = 44, ErrorMessage = "A chave da NF-e deve ter 44 dígitos.")]
        public string ChaveNFe { get; set; } = string.Empty;

        /// <summary>
        /// Código IBGE do município de descarga (RM03) — decide o agrupamento em
        /// <c>infMunDescarga</c>. Obrigatório: o XML não tem como agrupar sem ele.
        /// </summary>
        [Required(ErrorMessage = "O município de descarga é obrigatório.")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "O código IBGE deve ter 7 dígitos.")]
        public string CodMunDescarga { get; set; } = string.Empty;

        /// <summary>
        /// Nome do município de descarga (<c>infMunDescarga.xMunDescarga</c>).
        /// Obrigatório no leiaute e não derivável do código — não há tabela de
        /// municípios no sistema, então o nome viaja junto com o código.
        /// </summary>
        [Required(ErrorMessage = "O nome do município de descarga é obrigatório.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "O nome do município deve ter entre 2 e 60 caracteres.")]
        public string MunicipioDescarga { get; set; } = string.Empty;

        [StringLength(3)]
        public string? Serie { get; set; }

        public long? Numero { get; set; }

        public DateTime DataEmissao { get; set; }

        [StringLength(60)]
        public string? PartnerName { get; set; }

        [StringLength(2)]
        public string? UfOrigem { get; set; }

        [StringLength(2)]
        public string? UfDestino { get; set; }

        public decimal ValorTotal { get; set; }

        public decimal ValorMercadoria { get; set; }

        public decimal PesoBruto { get; set; }
    }

    /// <summary>UF de percurso, com a posição no trajeto.</summary>
    public class MdfePercursoDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "A UF de percurso é obrigatória.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "A UF deve ter 2 letras.")]
        public string UfPercurso { get; set; } = string.Empty;

        public int Ordem { get; set; }
    }

    /// <summary>Veículo escalado: tração ou reboque.</summary>
    public class MdfeVeiculoDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "O veículo é obrigatório.")]
        public int IdVehicle { get; set; }

        [Required(ErrorMessage = "O papel do veículo é obrigatório.")]
        public MdfePapelVeiculo? Tipo { get; set; }

        // Para exibição na aba, sem round-trip ao cadastro.
        public string? LicensePlate { get; set; }
        public string? LicensingState { get; set; }
        public int? Tare { get; set; }
        public int? CapacityKg { get; set; }
    }

    /// <summary>
    /// Condutor (RM06). <see cref="IdClient"/> é opcional: o manifesto não pode
    /// depender de o motorista estar cadastrado para poder ser emitido.
    /// </summary>
    public class MdfeCondutorDto
    {
        public int? Id { get; set; }

        public int? IdClient { get; set; }

        [Required(ErrorMessage = "O nome do condutor é obrigatório.")]
        [StringLength(60, ErrorMessage = "O nome do condutor deve ter no máximo 60 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CPF do condutor é obrigatório.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "O CPF deve ter 11 dígitos.")]
        public string Cpf { get; set; } = string.Empty;
    }

    /// <summary>Detalhe completo do manifesto (tela de visualização).</summary>
    public class MdfeResponseDto
    {
        public int Id { get; set; }
        public int IdCompany { get; set; }

        /// <summary>Atribuídos pelo serviço a partir da configuração fiscal (RM09).</summary>
        public string Serie { get; set; } = string.Empty;
        public long Numero { get; set; }

        public string? ChaveAcesso { get; set; }
        public string? Protocolo { get; set; }
        public DateTime DataEmissao { get; set; }
        public string UfCarregamento { get; set; } = string.Empty;
        public string UfDescarregamento { get; set; } = string.Empty;
        public MdfeTipoEmitente TipoEmitente { get; set; }
        public MdfeModal Modal { get; set; }
        public MdfeTipoOperacao TipoOperacao { get; set; }
        public MdfeStatus StatusMdfe { get; set; }
        public bool Sent { get; set; }
        public int TryCount { get; set; }
        public string? ErrorMessage { get; set; }

        // ------------------------------------------------------------------
        // Transmissão e eventos. Ver os campos correspondentes em MdfeEmissao
        // para o significado de cada um.
        // ------------------------------------------------------------------

        /// <summary>Código de situação da SEFAZ (100 = autorizado).</summary>
        public int? CStat { get; set; }

        /// <summary>Motivo literal da SEFAZ.</summary>
        public string? XMotivo { get; set; }

        public string? Recibo { get; set; }
        public DateTime? DataAutorizacao { get; set; }
        public DateTime? DataEncerramento { get; set; }
        public string? ProtocoloEncerramento { get; set; }
        public DateTime? DataCancelamento { get; set; }
        public string? JustificativaCancelamento { get; set; }
        public int SequenciaEvento { get; set; }

        /// <summary>
        /// Calculado, e não uma coluna: a janela de cancelamento do MDF-e é de 24
        /// horas contadas da autorização, e a tela precisa saber disso para
        /// desabilitar o botão antes de o usuário tentar e ser recusado.
        /// </summary>
        public bool PodeCancelar { get; set; }

        /// <summary>Idem, para o encerramento — que só cabe em manifesto autorizado e ainda não encerrado.</summary>
        public bool PodeEncerrar { get; set; }

        /// <summary>
        /// O XML assinado. Devolvido no detalhe para a tela poder exibir na aba
        /// Resumo; o download é <c>GET /api/Mdfe/{id}/xml</c>, que evita carregar
        /// o XML inteiro em toda listagem.
        /// </summary>
        public string? XmlCompleto { get; set; }

        public decimal ValorTotal { get; set; }
        public decimal PesoBruto { get; set; }
        public int QuantidadeNFe { get; set; }

        public string CodMunCarregamento { get; set; } = string.Empty;
        public string MunCarregamento { get; set; } = string.Empty;
        public MdfeTipoCarga TipoCarga { get; set; }
        public string ProdutoPredominante { get; set; } = string.Empty;
        public string? InfoAdFisco { get; set; }
        public string? InfoComplementar { get; set; }

        public string? CodigoCIOT { get; set; }
        public int? ContratanteId { get; set; }
        public string? ContratanteNome { get; set; }
        public MdfeIndicadorPagamento IndicadorPagamento { get; set; }

        public string? PagamentoBanco { get; set; }
        public string? PagamentoAgencia { get; set; }
        public string? PagamentoCnpjIpef { get; set; }
        public string? PagamentoChavePix { get; set; }

        public int? IdVeiculoTracao { get; set; }
        public string? VeiculoTracaoPlaca { get; set; }

        public List<MdfeDocumentoDto> Documentos { get; set; } = new();
        public List<MdfePercursoDto> Percurso { get; set; } = new();
        public List<MdfeVeiculoDto> Veiculos { get; set; } = new();
        public List<MdfeCondutorDto> Condutores { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Checklist do que falta para o manifesto poder gerar XML — mesmo
        /// formato de <see cref="MdfEStatusResult"/>, para a tela usar um
        /// componente só.
        /// </summary>
        public MdfeValidacaoResult Validacao { get; set; } = new();
    }

    /// <summary>
    /// Linha da listagem. Traz a validação já resolvida pelo mesmo motivo de
    /// <see cref="VehicleListItemDto"/>: sem isso a tabela faria uma chamada por
    /// linha só para pintar o badge de situação.
    /// </summary>
    public class MdfeListItemDto
    {
        public int Id { get; set; }
        public string Serie { get; set; } = string.Empty;
        public long Numero { get; set; }
        public string? ChaveAcesso { get; set; }
        public DateTime DataEmissao { get; set; }
        public string UfCarregamento { get; set; } = string.Empty;
        public string UfDescarregamento { get; set; } = string.Empty;
        public MdfeTipoEmitente TipoEmitente { get; set; }
        public MdfeTipoOperacao TipoOperacao { get; set; }
        public MdfeStatus StatusMdfe { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal PesoBruto { get; set; }
        public int QuantidadeNFe { get; set; }
        public string? VeiculoTracaoPlaca { get; set; }
        public string? MunCarregamento { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Motivo da SEFAZ, para a listagem mostrar o porquê da recusa em vez de
        /// só o badge vermelho — sem ele o usuário teria de abrir cada manifesto
        /// rejeitado para descobrir o que houve.
        /// </summary>
        public int? CStat { get; set; }
        public string? XMotivo { get; set; }
    }

    /// <summary>
    /// Próximo número que o manifesto vai receber (RM09), para a tela exibir no
    /// cabeçalho antes de salvar. Somente leitura: não reserva o número — a
    /// reserva acontece na criação, em transação.
    /// </summary>
    public class MdfeProximoNumeroDto
    {
        public string? Serie { get; set; }
        public long ProximoNumero { get; set; }
    }

    /// <summary>
    /// Documento fiscal disponível para ser manifestado — item da área de
    /// pesquisa (aba Documentos).
    ///
    /// Carrega os campos de exibição da tabela do mockup (Tipo, Número, Chave,
    /// Emissão, Parceiro, UF Origem, UF Destino, Valor Total, Valor da
    /// Mercadoria, Peso) já resolvidos, porque as duas origens (venda e compra)
    /// têm formatos diferentes e a tela não deveria conhecer os dois.
    /// </summary>
    public class DocumentoElegivelDto
    {
        /// <summary>
        /// Id na origem: <c>NFeEmission.Id</c> para Saída, <c>Purchase.Id</c> para
        /// Entrada. Qual dos dois usar é decidido por <see cref="TipoDocumento"/>.
        /// </summary>
        public int Id { get; set; }

        public MdfeTipoDocumento TipoDocumento { get; set; }
        public string ChaveNFe { get; set; } = string.Empty;
        public string? Serie { get; set; }
        public long? Numero { get; set; }
        public DateTime DataEmissao { get; set; }
        public string? PartnerName { get; set; }
        public string? UfOrigem { get; set; }
        public string? UfDestino { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal ValorMercadoria { get; set; }

        /// <summary>
        /// Peso bruto calculado de <c>Product.PesoUnitario</c> × quantidade.
        /// Zero quando nenhum item tem peso cadastrado — a tela mostra 0,00 e o
        /// total do manifesto sai menor, o que é visível e corrigível; inventar
        /// um peso seria pior.
        /// </summary>
        public decimal PesoBruto { get; set; }

        /// <summary>
        /// Município de descarga sugerido (código IBGE), do parceiro da nota.
        /// Sugestão, não decisão: o usuário pode trocar antes de adicionar.
        /// </summary>
        public string? CodMunDescargaSugerido { get; set; }

        public string? MunicipioDescarga { get; set; }

        /// <summary>
        /// Por que a nota NÃO pode ser manifestada. Vazio quando pode.
        ///
        /// A nota continua voltando na pesquisa mesmo inelegível, com o motivo
        /// preenchido — esconder seria pior: o usuário ficaria procurando uma
        /// nota que existe e não entende por que não aparece.
        /// </summary>
        public List<string> Impedimentos { get; set; } = new();

        public bool Elegivel => Impedimentos.Count == 0;
    }

    /// <summary>
    /// Resultado do checklist do manifesto. Mesmo formato de
    /// <see cref="MdfEStatusResult"/>.
    ///
    /// <see cref="PendingItems"/> lista TODAS as pendências, não só a primeira:
    /// o objetivo é corrigir em uma passada, em vez de descobrir um item por
    /// tentativa.
    /// </summary>
    public class MdfeValidacaoResult
    {
        public bool IsReady { get; set; }
        public List<string> PendingItems { get; set; } = new();

        /// <summary>
        /// Avisos que NÃO bloqueiam a geração do XML — hoje só a RM11 (peso acima
        /// da capacidade do veículo). Separado de <see cref="PendingItems"/>
        /// porque o MDF-e não recusa por excesso de peso: quem recusa é a
        /// fiscalização, com balança. Bloquear aqui impediria uma operação
        /// legítima.
        /// </summary>
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Dados do encerramento do manifesto (<c>evEncMDFe</c>).
    ///
    /// A UF e o município são os do ENCERRAMENTO — onde a viagem terminou —, que
    /// não são necessariamente os da descarga declarada. Por isso são parâmetros,
    /// e não algo derivado em silêncio: quando não vêm, o serviço usa o município
    /// de descarga do manifesto, que é o palpite certo na maioria dos casos, mas
    /// quem sabe a resposta é quem está encerrando.
    /// </summary>
    public class MdfeEncerrarDto
    {
        /// <summary>UF do encerramento. Vazio = usar a UF de descarregamento do manifesto.</summary>
        [StringLength(2)]
        public string? UfEncerramento { get; set; }

        /// <summary>Código IBGE do município de encerramento. Nulo = usar o município de descarga do manifesto.</summary>
        [StringLength(7)]
        public string? CodigoMunicipioEncerramento { get; set; }
    }

    /// <summary>
    /// Dados do cancelamento do manifesto (<c>evCancMDFe</c>).
    ///
    /// A justificativa é validada aqui — 15 a 255 caracteres, como a SEFAZ exige —
    /// em vez de deixar a SEFAZ recusar: o usuário recebe "a justificativa precisa
    /// de pelo menos 15 caracteres" em vez de um <c>cStat</c> de rejeição que não
    /// diz o que fazer.
    /// </summary>
    public class MdfeCancelarDto
    {
        [Required(ErrorMessage = "A justificativa do cancelamento é obrigatória.")]
        [StringLength(255, MinimumLength = 15, ErrorMessage = "A justificativa do cancelamento deve ter de 15 a 255 caracteres.")]
        public string Justificativa { get; set; } = string.Empty;
    }

    /// <summary>
    /// Situação do serviço do MDF-e na SEFAZ (<c>consStatServMDFe</c>). É o que se
    /// consulta ANTES de transmitir: com o serviço parado, a transmissão falha de
    /// um jeito que parece erro do manifesto.
    /// </summary>
    public class MdfeStatusServicoDto
    {
        public int CStat { get; set; }
        public string? XMotivo { get; set; }
        public DateTime DhRecbto { get; set; }
        public int? TMed { get; set; }
        public string? XObs { get; set; }
        public string? VersaoAplicativo { get; set; }
        public string? Ambiente { get; set; }
    }
}
