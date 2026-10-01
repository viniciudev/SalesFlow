namespace Model.Enums
{
    /// <summary>
    /// Tipo de emitente do MDF-e (<c>ide.tpEmit</c> no leiaute
    /// <c>mdfe_v3.00.xsd</c>).
    ///
    /// É o campo que decide o conjunto de regras aplicado na emissão:
    /// <list type="bullet">
    ///   <item><b>PST</b> — presta serviço de transporte para terceiros. Exige o
    ///   RNTRC do emitente em <c>emit</c> (RM08) e traz o grupo <c>infANTT</c>
    ///   com <c>infContratante</c>/<c>infCIOT</c>.</item>
    ///   <item><b>Carga própria</b> — transporta carga própria. Não exige RNTRC
    ///   no emitente nem o grupo <c>infANTT</c>; o RNTRC que aparece é o do
    ///   proprietário do veículo (<c>veicTracao.prop.RNTRC</c>), para o caso de
    ///   veículo de terceiro.</item>
    /// </list>
    ///
    /// Serialização NUMÉRICA, como <see cref="VehicleType"/> — o valor do enum é
    /// o valor do XML, então não pode haver renomeação sem quebrar contrato.
    /// </summary>
    public enum MdfeTipoEmitente
    {
        /// <summary>Prestador de Serviço de Transporte (tpEmit = 1).</summary>
        Pst = 1,

        /// <summary>Transportador de Carga Própria (tpEmit = 2).</summary>
        CargaPropria = 2
    }

    /// <summary>
    /// Modal do manifesto (<c>ide.modal</c>).
    ///
    /// Só o rodoviário está implementado (fase 1). Os outros valores existem no
    /// leiaute e ficam declarados para o campo não precisar de migration nova
    /// quando entrarem — mas escolhê-los é bloqueado na validação, em vez de
    /// gerar um XML que a SEFAZ recusaria por falta do grupo do modal.
    /// </summary>
    public enum MdfeModal
    {
        Rodoviario = 1,
        Aereo = 2,
        Aquaviario = 3,
        Ferroviario = 4
    }

    /// <summary>
    /// Classificação INTERNA da operação, derivada das notas selecionadas
    /// (RM14). Não existe no leiaute — o XSD não tem esse campo.
    ///
    /// Serve para as regras de negócio da ParaMadeiras:
    /// <list type="bullet">
    ///   <item><b>EntradaTerceiros</b> — NF-e de entrada, emitida por terceiro,
    ///   que a empresa recebe. O transporte vai do fornecedor (PE) para a
    ///   empresa (PI).</item>
    ///   <item><b>SaidaPropria</b> — NF-e de saída, emitida pela própria
    ///   empresa, que segue para outro estado.</item>
    /// </list>
    ///
    /// Documentos das duas naturezas no mesmo manifesto é operação mista: o
    /// serviço exige que o usuário escolha explicitamente, em vez de adivinhar.
    /// </summary>
    public enum MdfeTipoOperacao
    {
        /// <summary>NF-e de entrada (terceiros) — transporte de fornecedor para a empresa.</summary>
        EntradaTerceiros = 1,

        /// <summary>NF-e de saída (própria) — transporte da empresa para outro estado.</summary>
        SaidaPropria = 2
    }

    /// <summary>
    /// Situação da emissão no ServiceBOX. Não existe no leiaute — o que a SEFAZ
    /// devolve é o <c>cStat</c> do protocolo, tratado à parte.
    ///
    /// Os valores 3 a 5 foram declarados com o número definitivo desde a criação
    /// da tabela, para que a transmissão e os eventos (encerramento e
    /// cancelamento) não precisassem de migration só para remapear status.
    /// </summary>
    public enum MdfeStatus
    {
        /// <summary>Rascunho: documentos e veículo escolhidos, XML ainda não gerado.</summary>
        Rascunho = 1,

        /// <summary>
        /// XML montado, ASSINADO e aprovado na validação contra os XSDs locais.
        /// Não significa autorizado pela SEFAZ — é o estado de onde a transmissão
        /// parte.
        /// </summary>
        Validado = 2,

        /// <summary>Autorizado pela SEFAZ: tem protocolo e <c>DataAutorizacao</c>.</summary>
        Autorizado = 3,

        /// <summary>Cancelado por evento <c>evCancMDFe</c>.</summary>
        Cancelado = 4,

        /// <summary>Encerrado por evento <c>evEncMDFe</c>.</summary>
        Encerrado = 5,

        /// <summary>Falha na montagem, assinatura ou validação — ver <c>ErrorMessage</c>.</summary>
        Erro = 6
    }

    /// <summary>
    /// Papel do veículo dentro do manifesto — mesma semântica de
    /// <see cref="VehicleType"/>, mas do ponto de vista da linha de
    /// <c>tb_mdfeVeiculo</c>, que é a escala do manifesto e não do cadastro.
    ///
    /// Existe separado de <see cref="VehicleType"/> de propósito: o cadastro diz
    /// o que o veículo É, e a escala diz o que ele FEZ naquela viagem — um
    /// reboque cadastrado pode, em tese, ser escalado como tração em outro
    /// manifesto, e amarrar os dois enums faria essa distinção sumir.
    /// </summary>
    public enum MdfePapelVeiculo
    {
        /// <summary>Veículo de tração (cavalo mecânico, truck, toco, VAN...).</summary>
        Tracao = 1,

        /// <summary>
        /// Reboque / semirreboque. O <c>mdfeModalRodoviario_v3.00.xsd</c> limita a
        /// <c>maxOccurs="3"</c> — três reboques, e não cinco como o MOC de versões
        /// antigas dizia. O limite é conferido no <c>MdfeBuilder</c>.
        /// </summary>
        Reboque = 2
    }

    /// <summary>
    /// Natureza do documento fiscal vinculado ao manifesto, para separar as duas
    /// origens de dados que o sistema tem:
    /// <list type="bullet">
    ///   <item><b>Saida</b> — <c>NFeEmission</c>, a NF-e que o próprio sistema
    ///   emitiu (venda).</item>
    ///   <item><b>Entrada</b> — <c>Purchase</c>, a NF-e de terceiro importada via
    ///   XML de compra.</item>
    /// </list>
    ///
    /// Serve também para a checagem de coerência com
    /// <see cref="MdfeTipoOperacao"/>: um manifesto de carga própria não pode
    /// levar nota de entrada.
    /// </summary>
    public enum MdfeTipoDocumento
    {
        /// <summary>NF-e de entrada (compra de terceiros) — origem <c>Purchase</c>.</summary>
        Entrada = 1,

        /// <summary>NF-e de saída (venda própria) — origem <c>NFeEmission</c>.</summary>
        Saida = 2
    }

    /// <summary>
    /// Tipo de carga do produto predominante (<c>prodPred.tpCarga</c> no leiaute).
    ///
    /// Os doze valores são o domínio fechado do MOC e não podem ser renomeados
    /// sem quebrar contrato — o número do enum É o valor do XML (com dois
    /// dígitos: <c>05</c>, não <c>5</c>).
    ///
    /// Para a ParaMadeiras o valor real é <see cref="CargaGeral"/> (madeira
    /// serrada). Os demais existem para o campo não mentir se a operação mudar —
    /// escolher o tipo errado é rejeição na SEFAZ, então o serviço deixa o
    /// usuário decidir em vez de deduzir do produto.
    /// </summary>
    public enum MdfeTipoCarga
    {
        GranelSolido = 1,
        GranelLiquido = 2,
        Frigorificada = 3,
        Conteinerizada = 4,
        CargaGeral = 5,
        Neogranel = 6,
        PerigosaGranelSolido = 7,
        PerigosaGranelLiquido = 8,
        PerigosaCargaFrigorificada = 9,
        PerigosaCargaConteinerizada = 10,
        PerigosaCargaGeral = 11,
        GranelPressurizada = 12
    }

    /// <summary>
    /// Forma de pagamento do frete (<c>infANTT.infPag.indPag</c>), usada só no
    /// grupo <c>infPag</c> — que por sua vez só existe para PST.
    ///
    /// Os nomes seguem o leiaute (0 à vista, 1 a prazo) e o valor é o do XML.
    /// </summary>
    public enum MdfeIndicadorPagamento
    {
        /// <summary>Pagamento à vista (indPag = 0).</summary>
        AVista = 0,

        /// <summary>Pagamento a prazo (indPag = 1).</summary>
        APrazo = 1
    }
}
