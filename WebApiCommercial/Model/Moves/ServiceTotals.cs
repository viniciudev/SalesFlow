#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Moves
{
    /// <summary>
    /// Cálculo único dos totais de OS e NFS-e.
    ///
    /// Existe para que <c>ServiceOrderService</c> e <c>ServiceInvoiceService</c> não possam
    /// divergir: os dois chamam estes mesmos métodos.
    ///
    /// Duas regras fiscais estão embutidas aqui e não são óbvias:
    ///
    /// 1. A base do ISS (e das demais retenções) é (quantidade x valor unitário) - desconto.
    ///    O desconto modelado é o INCONDICIONAL (LC 116/2003, art. 7º). O padrão Nacional
    ///    separa desconto incondicional de condicional; aqui só o primeiro existe.
    ///
    /// 2. O ISS só sai do líquido quando <c>IssqnRetido</c> é true. ISS não retido é obrigação
    ///    do prestador, não do tomador, e portanto NÃO pode reduzir o valor líquido a receber.
    ///    Tratar ISS não retido como redutor do líquido é o erro mais comum neste domínio.
    ///
    /// Todo arredondamento é feito POR ITEM e só depois somado. Arredondar a soma de valores
    /// não arredondados é como nascem as rejeições por divergência de centavos.
    /// </summary>
    public static class ServiceTotals
    {
        /// <summary>Arredonda para centavos, com o critério comercial (0,005 sobe).</summary>
        public static decimal R2(decimal value) =>
            Math.Round(value, 2, MidpointRounding.AwayFromZero);

        /// <summary>Valores calculados de um item.</summary>
        public readonly record struct ItemAmounts(
            decimal Gross,
            decimal Discount,
            decimal Base,
            decimal Issqn,
            decimal Pis,
            decimal Cofins,
            decimal Ir,
            decimal Csll,
            decimal Inss,
            bool IssqnRetido)
        {
            /// <summary>Retenções federais do item (PIS/COFINS/IR/CSLL/INSS).</summary>
            public decimal FederalRetentions => R2(Pis + Cofins + Ir + Csll + Inss);

            /// <summary>Parcela do ISS que efetivamente sai do líquido (zero quando não retido).</summary>
            public decimal IssqnRetidoValue => IssqnRetido ? Issqn : 0m;
        }

        /// <summary>Totais consolidados de um conjunto de itens.</summary>
        public readonly record struct Result(
            decimal Subtotal,
            decimal DiscountValue,
            decimal BaseTotal,
            decimal IssqnValue,
            decimal IssqnRetidoValue,
            decimal RetentionValue,
            decimal NetValue);

        public static ItemAmounts ComputeItem(
            decimal quantity,
            decimal unitPrice,
            decimal discount,
            decimal issqnRate,
            bool issqnRetido,
            decimal pisRate,
            decimal cofinsRate,
            decimal irRate,
            decimal csllRate,
            decimal inssRate)
        {
            var gross = R2(quantity * unitPrice);
            var discountValue = R2(discount);
            var net = R2(gross - discountValue);

            return new ItemAmounts(
                Gross: gross,
                Discount: discountValue,
                Base: net,
                Issqn: R2(net * issqnRate / 100m),
                Pis: R2(net * pisRate / 100m),
                Cofins: R2(net * cofinsRate / 100m),
                Ir: R2(net * irRate / 100m),
                Csll: R2(net * csllRate / 100m),
                Inss: R2(net * inssRate / 100m),
                IssqnRetido: issqnRetido);
        }

        public static ItemAmounts ForItem(ServiceOrderItem item) =>
            ComputeItem(item.Quantity, item.UnitPrice, item.Discount, item.IssqnRate, item.IssqnRetido,
                item.PisRate, item.CofinsRate, item.IrRate, item.CsllRate, item.InssRate);

        public static ItemAmounts ForItem(ServiceInvoiceItem item) =>
            ComputeItem(item.Quantity, item.UnitPrice, item.Discount, item.IssqnRate, item.IssqnRetido,
                item.PisRate, item.CofinsRate, item.IrRate, item.CsllRate, item.InssRate);

        /// <summary>
        /// Consolida os itens. <c>Subtotal</c> é a soma dos brutos (preserva o significado
        /// histórico de TotalValue); <c>NetValue</c> é a base menos todas as retenções.
        /// </summary>
        public static Result Sum(IEnumerable<ItemAmounts> items)
        {
            var list = items as IList<ItemAmounts> ?? items.ToList();

            var subtotal = list.Sum(i => i.Gross);
            var discountValue = list.Sum(i => i.Discount);
            var baseTotal = list.Sum(i => i.Base);
            var issqnValue = list.Sum(i => i.Issqn);
            var issqnRetidoValue = list.Sum(i => i.IssqnRetidoValue);
            var retentionValue = R2(list.Sum(i => i.FederalRetentions) + issqnRetidoValue);

            return new Result(
                Subtotal: subtotal,
                DiscountValue: discountValue,
                BaseTotal: baseTotal,
                IssqnValue: issqnValue,
                IssqnRetidoValue: issqnRetidoValue,
                RetentionValue: retentionValue,
                NetValue: R2(baseTotal - retentionValue));
        }

        public static Result ForOrder(IEnumerable<ServiceOrderItem> items) =>
            Sum(items.Select(ForItem));

        public static Result ForInvoice(IEnumerable<ServiceInvoiceItem> items) =>
            Sum(items.Select(ForItem));

        /// <summary>Aplica os totais consolidados a uma OS.</summary>
        public static void Apply(ServiceOrder order)
        {
            var t = ForOrder(order.ServiceOrderItems);
            order.TotalValue = t.Subtotal;
            order.DiscountValue = t.DiscountValue;
            order.IssqnValue = t.IssqnValue;
            order.IssqnRetidoValue = t.IssqnRetidoValue;
            order.RetentionValue = t.RetentionValue;
            order.NetValue = t.NetValue;
        }

        /// <summary>Aplica os totais consolidados a uma NFS-e.</summary>
        public static void Apply(ServiceInvoice invoice)
        {
            var t = ForInvoice(invoice.ServiceInvoiceItems);
            invoice.TotalValue = t.Subtotal;
            invoice.DiscountValue = t.DiscountValue;
            invoice.IssqnValue = t.IssqnValue;
            invoice.IssqnRetidoValue = t.IssqnRetidoValue;
            invoice.RetentionValue = t.RetentionValue;
            invoice.NetValue = t.NetValue;
        }
    }
}
