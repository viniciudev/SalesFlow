using Model.Registrations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Moves
{
	public class PurchaseItem : BaseEntity
	{
		public int CompraId { get; set; }
		public Purchase Compra { get; set; }
		public int? ProdutoId { get; set; }
		public Product Produto { get; set; }
		public string CodigoProduto { get; set; }
		public string DescricaoProduto { get; set; }
		public decimal Quantidade { get; set; }
	
		public decimal ValorUnitario { get; set; }
		public decimal Desconto { get; set; }
		public decimal ValorTotal { get; set; }

		// ===== FATOR DE CONVERSAO (origem da linha) =====
		/// <summary>Unidade comercial informada no XML (uCom). Ex.: KG, UN, CX.</summary>
		public string? Unidade { get; set; }
		/// <summary>Quantidade original do XML (qCom), antes da conversao.</summary>
		public decimal? QuantidadeXml { get; set; }
		/// <summary>Valor unitario original do XML (vUnCom), antes da conversao.</summary>
		public decimal? ValorUnitarioXml { get; set; }
		/// <summary>
		/// Fator multiplicador (default 1): Quantidade = QuantidadeXml * FatorConversao;
		/// ValorUnitario = ValorUnitarioXml / FatorConversao. Linhas manuais usam 1.
		/// </summary>
		public decimal? FatorConversao { get; set; }

		[NotMapped]
		public string NomeProduto { get; set; }
	}
}
