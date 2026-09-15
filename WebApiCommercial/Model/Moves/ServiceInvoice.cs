#nullable enable
using Model.Enums;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Moves
{
    public class ServiceInvoice : BaseEntity
    {
        public int ServiceOrderId { get; set; }
        public ServiceOrder? ServiceOrder { get; set; }
        public int TenantId { get; set; }
        public Company? Company { get; set; }
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        /// <summary>
        /// Identificador da DPS no padrão Nacional:
        /// "DPS" + cLocEmi(7) + tipoInsc(1) + inscricaoFederal(14) + serie(5) + numero(15) = 45 caracteres.
        /// Fica nulo até a emissão (é atribuído junto com <see cref="NumeroDPS"/>).
        /// </summary>
        public string? IdDPS { get; set; }

        /// <summary>
        /// Chave de acesso da NFS-e (50 dígitos), devolvida pelo SEFIN na autorização.
        /// </summary>
        public string? ChaveAcesso { get; set; }

        public AmbienteEnum TipoAmbiente { get; set; }
        public DateTime? DhEmissao { get; set; }
        public string? CodMunIBGE { get; set; }
        public ServiceInvoiceStatus Status { get; set; }
        public int NumeroDPS { get; set; }
        public DateTime DataCompetencia { get; set; }

        /// <summary>Soma dos totais brutos dos itens, antes de descontos.</summary>
        public decimal TotalValue { get; set; }

        /// <summary>Soma dos descontos incondicionais dos itens.</summary>
        public decimal DiscountValue { get; set; }

        /// <summary>Soma do ISS de todos os itens, retido ou não.</summary>
        public decimal IssqnValue { get; set; }

        /// <summary>Parcela do ISS efetivamente retida pelo tomador.</summary>
        public decimal IssqnRetidoValue { get; set; }

        /// <summary>Soma de todas as retenções (PIS/COFINS/IR/CSLL/INSS + ISS retido).</summary>
        public decimal RetentionValue { get; set; }

        /// <summary>
        /// Líquido a receber. Atenção: é conceito NOSSO (financeiro e DANFSE) — o padrão
        /// Nacional não tem um campo "valor líquido" na DPS (ValoresServico só carrega
        /// vServ e vReceb). O líquido é implícito: vServ menos os grupos de retenção.
        /// </summary>
        public decimal NetValue { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? EmittedAt { get; set; }
        public string? CancelReason { get; set; }
        public Guid? CanceledBy { get; set; }
        public Guid CreatedBy { get; set; }

        // === Retorno da transmissão (espelha Model/NFe/NFeEmission.cs) ===

        /// <summary>
        /// Reservado por paridade com NFeEmission. O padrão Nacional não devolve protocolo
        /// numérico — a resposta traz apenas IdDps, ChaveAcesso e XmlNFSe. Fica nulo.
        /// </summary>
        public string? Protocolo { get; set; }

        /// <summary>XML da NFS-e autorizada.</summary>
        [Column(TypeName = "text")]
        public string? XmlNfse { get; set; }

        /// <summary>true somente quando o SEFIN autorizou.</summary>
        public bool Sent { get; set; }

        /// <summary>Quantas vezes a emissão foi tentada (para diagnóstico e retentativa).</summary>
        public int TryCount { get; set; }

        /// <summary>Mensagem de erro da última tentativa, quando não autorizada.</summary>
        [Column(TypeName = "text")]
        public string? ErrorMessage { get; set; }

        /// <summary>XML assinado enviado na última tentativa.</summary>
        [Column(TypeName = "text")]
        public string? RequestPayloadJson { get; set; }

        /// <summary>Retorno cru do SEFIN na última tentativa, para diagnóstico.</summary>
        [Column(TypeName = "text")]
        public string? ResponseJson { get; set; }

        public ICollection<ServiceInvoiceItem> ServiceInvoiceItems { get; set; } = new List<ServiceInvoiceItem>();
    }
}
