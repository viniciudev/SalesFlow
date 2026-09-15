#nullable enable
using Model.Registrations;
using System;

namespace Model.Moves
{
    public class ServiceInvoiceItem : BaseEntity
    {
        public int ServiceInvoiceId { get; set; }
        public ServiceInvoice? ServiceInvoice { get; set; }
        public int ServiceProvidedId { get; set; }
        public ServiceProvided? ServiceProvided { get; set; }

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Desconto incondicional do item. A base do ISS e das demais retenções é
        /// (Quantity x UnitPrice) - Discount.
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// Descrição que sobrepõe <see cref="ServiceProvided.Description"/> na DPS.
        /// Quando nula, vale a descrição do serviço cadastrado.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>Total do item já líquido do desconto: (Quantity x UnitPrice) - Discount.</summary>
        public decimal TotalPrice { get; set; }

        // === Alíquotas (percentuais) — copiadas da OS na criação da NFS-e ===

        public decimal IssqnRate { get; set; }

        /// <summary>
        /// Quando true, o ISS é retido pelo tomador e por isso sai do líquido.
        /// Quando false, o ISS é obrigação do prestador e NÃO reduz o líquido.
        /// </summary>
        public bool IssqnRetido { get; set; }

        public decimal PisRate { get; set; }
        public decimal CofinsRate { get; set; }
        public decimal IrRate { get; set; }
        public decimal CsllRate { get; set; }
        public decimal InssRate { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
