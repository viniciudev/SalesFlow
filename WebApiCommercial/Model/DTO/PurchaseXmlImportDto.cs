using System.Collections.Generic;

namespace Model.DTO
{
	public class XmlImportResultDto
	{
		public ProviderDto Fornecedor { get; set; }
		public List<ProductImportDto> Produtos { get; set; } = new List<ProductImportDto>();
		public PurchaseImportDto Compra { get; set; }
		public List<PurchaseItemImportDto> Itens { get; set; } = new List<PurchaseItemImportDto>();
		public bool IsHomologacao { get; set; }
		public string ChaveNfe { get; set; }
		public List<string> Avisos { get; set; } = new List<string>();
		public List<string> Erros { get; set; } = new List<string>();
	}

	public class PurchaseImportDto
	{
		public int Id { get; set; }
		public int IdCompany { get; set; }
		public string DataEntrada { get; set; }
		public string DataCompra { get; set; }
		public string ChaveNfe { get; set; }
		public int FornecedorId { get; set; }
		public decimal ValorTotal { get; set; }
		public string Serie { get; set; }
		public string Numero { get; set; }

		/// <summary>Detalhamento de custos/impostos extraido dos totais da NF-e.</summary>
		public PurchaseCostsDto? Custos { get; set; }

		/// <summary>Resumo legivel dos custos (gravado na Observacao da compra).</summary>
		public string? Observacao { get; set; }
	}

	public class PurchaseItemImportDto
	{
		public int? ProdutoId { get; set; }
		public string CodigoProduto { get; set; }
		public string DescricaoProduto { get; set; }
		public decimal Quantidade { get; set; }
		public decimal ValorUnitario { get; set; }
		public decimal Desconto { get; set; }
		public decimal ValorTotal { get; set; }
		public bool ProdutoCriado { get; set; }

		// ===== FATOR DE CONVERSAO (origem da linha) =====
		public string? Unidade { get; set; }
		public decimal? QuantidadeXml { get; set; }
		public decimal? ValorUnitarioXml { get; set; }
		public decimal? FatorConversao { get; set; }
	}

	public class ProductImportDto
	{
		public int Id { get; set; }
		public int IdCompany { get; set; }
		public string Name { get; set; }
		public string Code { get; set; }
		public string Reference { get; set; }
		public string Ncm { get; set; }
		public string Cest { get; set; }
		public decimal CostPrice { get; set; }
		public bool CriadoNaImportacao { get; set; }

		/// <summary>Peso (kg) por unidade do produto encontrado/criado no cadastro.</summary>
		public decimal? PesoUnitario { get; set; }
	}
}
