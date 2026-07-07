using Model.Moves;
using System;
using System.Collections.Generic;

namespace Model.Registrations
{
	public class Product : BaseEntity
	{
		public int IdCompany { get; set; }
		public Company Company { get; set; }
		[Uppercase]
		public string Name { get; set; }
		public decimal Value { get; set; }
		public decimal Quantity { get; set; }
		public string Description { get; set; }
		public string Observation { get; set; } = string.Empty;
		public byte[]? Image { get; set; }
		public ICollection<SaleItems> SaleItems { get; set; }
		public ICollection<Commission> Commissions { get; set; }
		public ICollection<Financial> Financials { get; set; }
		public ICollection<Stock> Stocks { get; set; } = new List<Stock>();

		public string Code { get; set; }
		public string ImageBytes { get; set; }
		public string Reference { get; set; }
		public decimal CostPrice { get; set; }
		public string Ncm { get; set; }
		public ICollection<PurchaseItem> PurchaseItems { get; set; }

		// ========== MATRIZ TRIBUTARIA ==========

		/// <summary>
		/// Situacao tributaria do produto (ex: NORMAL, ST_RETIDA).
		/// Utilizada em conjunto com NaturezaOperacao + Destino para resolver a RegraFiscal.
		/// </summary>
		public int? SituacaoTributariaId { get; set; }

		/// <summary>
		/// Navegacao para a situacao tributaria.
		/// </summary>
		public SituacaoTributaria? SituacaoTributaria { get; set; }

		// ========== TRIBUTACAO POR PRODUTO (deprecated - mantido para compatibilidade) ==========

		/// <summary>
		/// Quando true, este produto usa sua propria ConfiguracaoTributaria.
		/// Deprecated: Substituido pelo modelo de matriz tributaria (RegraFiscal).
		/// Mantido para compatibilidade com registros existentes.
		/// </summary>
		public bool UsaTributacaoPropria { get; set; } = false;

		/// <summary>
		/// Configuracao tributaria especifica do produto.
		/// Deprecated: Substituido pelo modelo de matriz tributaria.
		/// Mantido para compatibilidade com registros existentes.
		/// </summary>
		public ConfiguracaoTributaria? ConfiguracaoTributaria { get; set; }

		/// <summary>
		/// ID da Natureza de Operacao de origem (quando a config foi clonada).
		/// Deprecated: Mantido para compatibilidade.
		/// </summary>
		public int? NaturezaOperacaoOrigemId { get; set; }

		/// <summary>
		/// Data da ultima modificacao da configuracao tributaria.
		/// </summary>
		public DateTime? DataAtualizacaoTributaria { get; set; }
		public string? Cest { get; set; }
	}
}
