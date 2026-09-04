using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Moves
{
	public class Purchase : BaseEntity
	{
		public int IdCompany { get; set; }
		public Company Company { get; set; }
		public DateTime DataEntrada { get; set; }
		public DateTime DataCompra { get; set; }
		public string ChaveNfe { get; set; }
		public int FornecedorId { get; set; }
		public Provider Fornecedor { get; set; }
		public decimal ValorTotal { get; set; }
		public DateTime DataCadastro { get; set; } = DateTime.Now;
		public ICollection<PurchaseItem> PurchaseItems { get; set; }
		public ICollection<Financial> Financials { get; set; }

		// ===== DETALHAMENTO DE CUSTOS / IMPOSTOS (extraidos do XML de entrada) =====
		// Obs.: ValorTotal permanece a soma dos itens; os campos abaixo sao informativos.

		/// <summary>vProd — valor dos produtos</summary>
		public decimal? ValorProdutos { get; set; }
		/// <summary>vFrete</summary>
		public decimal? ValorFrete { get; set; }
		/// <summary>vSeg</summary>
		public decimal? ValorSeguro { get; set; }
		/// <summary>vDesc</summary>
		public decimal? ValorDesconto { get; set; }
		/// <summary>vIPI</summary>
		public decimal? ValorIPI { get; set; }
		/// <summary>vPIS</summary>
		public decimal? ValorPIS { get; set; }
		/// <summary>vCOFINS</summary>
		public decimal? ValorCOFINS { get; set; }
		/// <summary>vICMS</summary>
		public decimal? ValorICMS { get; set; }
		/// <summary>vIBS (reforma tributaria)</summary>
		public decimal? ValorIBS { get; set; }
		/// <summary>vCBS (reforma tributaria)</summary>
		public decimal? ValorCBS { get; set; }
		/// <summary>vBC — base de calculo ICMS</summary>
		public decimal? BaseCalculoICMS { get; set; }
		/// <summary>vBCIBSCBS — base de calculo IBS/CBS</summary>
		public decimal? BaseCalculoIBSCBS { get; set; }
		/// <summary>vNF — total da nota fiscal</summary>
		public decimal? ValorNotaFiscal { get; set; }
		/// <summary>vTotTrib — total de tributos</summary>
		public decimal? ValorTotalTributos { get; set; }

		/// <summary>
		/// Extras nao mapeados em colunas (ex.: vOutro, vII, vFCP, vST, vFCPST, vICMSDeson...)
		/// serializados como JSON (chave -&gt; valor).
		/// </summary>
		public string? CustosExtrasJson { get; set; }

		/// <summary>Observacao / resumo legivel dos custos da compra.</summary>
		public string? Observacao { get; set; }

		[NotMapped]
		public string NomeFornecedor { get; set; }
	}
}
