using System.Collections.Generic;

namespace Model.DTO
{
    /// <summary>
    /// Detalhamento de custos/impostos de uma compra, extraido dos totais do XML
    /// de entrada (total/ICMSTot + total/IBSCBSTot). Persistido em colunas de tb_purchase.
    /// </summary>
    public class PurchaseCostsDto
    {
        /// <summary>vProd — valor dos produtos.</summary>
        public decimal ValorProdutos { get; set; }

        /// <summary>vFrete.</summary>
        public decimal ValorFrete { get; set; }

        /// <summary>vSeg.</summary>
        public decimal ValorSeguro { get; set; }

        /// <summary>vDesc (positivo no XML; exibido como desconto).</summary>
        public decimal ValorDesconto { get; set; }

        /// <summary>vIPI.</summary>
        public decimal ValorIPI { get; set; }

        /// <summary>vPIS.</summary>
        public decimal ValorPIS { get; set; }

        /// <summary>vCOFINS.</summary>
        public decimal ValorCOFINS { get; set; }

        /// <summary>vICMS.</summary>
        public decimal ValorICMS { get; set; }

        /// <summary>vIBS (reforma tributaria).</summary>
        public decimal ValorIBS { get; set; }

        /// <summary>vCBS (reforma tributaria).</summary>
        public decimal ValorCBS { get; set; }

        /// <summary>vBC — base de calculo do ICMS.</summary>
        public decimal BaseCalculoICMS { get; set; }

        /// <summary>vBCIBSCBS — base de calculo de IBS/CBS.</summary>
        public decimal BaseCalculoIBSCBS { get; set; }

        /// <summary>vNF — total da nota fiscal.</summary>
        public decimal ValorTotal { get; set; }

        /// <summary>vTotTrib — total estimado de tributos.</summary>
        public decimal ValorTotalTributos { get; set; }

        /// <summary>
        /// Valores presentes no XML sem coluna propria (ex.: vOutro, vII, vFCP, vST,
        /// vFCPST, vICMSDeson, vIPIDevol). Chave = elemento do XML. Serializados na
        /// coluna CustosExtrasJson de tb_purchase.
        /// </summary>
        public Dictionary<string, decimal> OutrosCustos { get; set; } = new Dictionary<string, decimal>();
    }
}
