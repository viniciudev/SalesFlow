#nullable enable
using Model.Enums;
using Model.Registrations;
using System;
using System.Collections.Generic;

namespace Model.Moves
{
    public class ServiceOrder : BaseEntity
    {
        public int TenantId { get; set; }
        public Company? Company { get; set; }
        public int ClientId { get; set; }
        public Client? Client { get; set; }
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Observações livres da OS.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Competência sugerida (mês de prestação). Serve de valor inicial para
        /// <see cref="ServiceInvoice.DataCompetencia"/> no momento da emissão; a competência
        /// que vale fiscalmente é a da NFS-e, não esta.
        /// </summary>
        public DateTime? Competence { get; set; }

        /// <summary>
        /// Soma dos totais brutos dos itens (quantidade x valor unitário), antes de descontos.
        /// Mantém o significado histórico do campo.
        /// </summary>
        public decimal TotalValue { get; set; }

        /// <summary>Soma dos descontos incondicionais dos itens.</summary>
        public decimal DiscountValue { get; set; }

        /// <summary>Soma do ISS de todos os itens, retido ou não.</summary>
        public decimal IssqnValue { get; set; }

        /// <summary>Parcela do ISS efetivamente retida pelo tomador.</summary>
        public decimal IssqnRetidoValue { get; set; }

        /// <summary>Soma de todas as retenções (PIS/COFINS/IR/CSLL/INSS + ISS retido).</summary>
        public decimal RetentionValue { get; set; }

        /// <summary>Líquido a receber: base (bruto - descontos) menos as retenções.</summary>
        public decimal NetValue { get; set; }

        public ServiceOrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ConcludedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }

        public ICollection<ServiceOrderItem> ServiceOrderItems { get; set; } = new List<ServiceOrderItem>();
        public ICollection<ServiceInvoice> ServiceInvoices { get; set; } = new List<ServiceInvoice>();
    }
}
