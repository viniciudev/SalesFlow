using Model.Enums;
using Model.Moves;
using Model.Registrations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Model.MDFe
{
    /// <summary>
    /// Uma NF-e manifestada — o vínculo entre o manifesto e a nota que ele
    /// transporta.
    ///
    /// As duas origens possíveis (<see cref="NFeEmissionId"/> para nota de venda
    /// emitida aqui, <see cref="PurchaseId"/> para nota de compra importada de
    /// terceiro) são representadas por DUAS FKs anuláveis, e não por um par
    /// (tipo, id) genérico: cada lado tem seu próprio repositório e suas próprias
    /// regras de elegibilidade, e uma FK polimórfica trocaria duas verificações
    /// de integridade referencial por zero.
    ///
    /// Os campos de exibição são SNAPSHOT do que foi manifestado, não espelho do
    /// cadastro: número, série, parceiro, UF e valores ficam gravados como
    /// estavam no momento da emissão. Sem isso, cancelar ou alterar uma NF-e
    /// depois mudaria retroativamente o que o manifesto declarou à SEFAZ — e o
    /// XML já assinado deixaria de bater com a tabela.
    /// </summary>
    public class MdfeDocumento : BaseEntity
    {
        public int IdMdfe { get; set; }
        public MdfeEmissao Mdfe { get; set; }

        /// <summary>Entrada (compra de terceiro) ou Saída (venda própria).</summary>
        public MdfeTipoDocumento TipoDocumento { get; set; }

        /// <summary>Nota de saída — preenchida quando <see cref="TipoDocumento"/> é Saída.</summary>
        public int? NFeEmissionId { get; set; }
        public NFeEmission? NFeEmission { get; set; }

        /// <summary>Nota de entrada — preenchida quando <see cref="TipoDocumento"/> é Entrada.</summary>
        public int? PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }

        /// <summary>Chave de acesso da NF-e (44 dígitos). RM02 exige que seja válida.</summary>
        [Required]
        [StringLength(44)]
        public string ChaveNFe { get; set; }

        /// <summary>
        /// Série e número, para exibição. Na nota de entrada vêm DA CHAVE
        /// (posições 22-25 e 26-34): a <c>Purchase</c> não tem esses campos
        /// próprios, porque a importação do XML de compra só guarda a chave.
        /// </summary>
        [StringLength(3)]
        public string? Serie { get; set; }

        public long? Numero { get; set; }

        public DateTime DataEmissao { get; set; }

        /// <summary>Razão social do emitente da nota (fornecedor na entrada, cliente na saída).</summary>
        [StringLength(60)]
        public string? PartnerName { get; set; }

        [StringLength(2)]
        public string? UfOrigem { get; set; }

        [StringLength(2)]
        public string? UfDestino { get; set; }

        /// <summary>
        /// Código IBGE do MUNICÍPIO de descarga (7 dígitos).
        ///
        /// É o campo que decide o agrupamento no XML: o leiaute organiza as notas
        /// em <c>infMunDescarga</c> (um grupo por município), não por UF. Duas
        /// notas do mesmo estado em cidades diferentes vão para grupos distintos.
        /// RM03 valida que todo documento tem município, e o serviço usa o
        /// município do parceiro como padrão quando a nota não traz um.
        /// </summary>
        [Required]
        [StringLength(7)]
        public string CodMunDescarga { get; set; }

        /// <summary>
        /// Nome do município de descarga (<c>infMunDescarga.xMunDescarga</c>).
        ///
        /// O leiaute exige o nome JUNTO do código, e o sistema não tem tabela de
        /// municípios para derivá-lo (a UF e a cidade moram no endereço do
        /// <see cref="Client"/>, não numa tabela própria) — então o nome é
        /// gravado aqui, como os demais campos de exibição desta tabela. Sem ele
        /// não há como montar o grupo, e buscá-lo no parceiro na hora de montar
        /// faria o XML depender de um cadastro que pode ter mudado depois da
        /// emissão.
        /// </summary>
        [Required]
        [StringLength(60)]
        public string MunicipioDescarga { get; set; }

        /// <summary>Valor total da NF-e (<c>infNFe.vNF</c>).</summary>
        public decimal ValorTotal { get; set; }

        /// <summary>Valor das mercadorias (<c>infNFe.vCarga</c>).</summary>
        public decimal ValorMercadoria { get; set; }

        /// <summary>
        /// Peso bruto da nota. Vem de <c>Product.PesoUnitario</c> × quantidade do
        /// item — o cadastro do produto é a única fonte de peso do sistema.
        /// </summary>
        public decimal PesoBruto { get; set; }
    }
}
