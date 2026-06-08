using System;
using System.Collections.Generic;

namespace Model.DTO
{
	public class PurchaseDto
	{
		public int Id { get; set; }
		public int IdCompany { get; set; }
		public DateTime DataEntrada { get; set; }
		public DateTime DataCompra { get; set; }
		public string ChaveNfe { get; set; }
		public int FornecedorId { get; set; }
		public decimal ValorTotal { get; set; }
		public List<PurchaseItemDto> PurchaseItems { get; set; } = new List<PurchaseItemDto>();
		public List<FormPaymentPurchase> FormPayment { get; set; } = new();
		public decimal? Troco { get; set; }
		public int? BankAccountId { get; set; }
	}

	public class PurchaseItemDto
	{
		public int Id { get; set; }
		public int CompraId { get; set; }
		public int? ProdutoId { get; set; }
		public string CodigoProduto { get; set; }
		public string DescricaoProduto { get; set; }
		public decimal Quantidade { get; set; }
		public decimal ValorUnitario { get; set; }
		public decimal Desconto { get; set; }
		public decimal ValorTotal { get; set; }
	}

	public class PurchaseListDto
	{
		public int Id { get; set; }
		public DateTime DataEntrada { get; set; }
		public DateTime DataCompra { get; set; }
		public string ChaveNfe { get; set; }
		public string NomeFornecedor { get; set; }
		public int FornecedorId { get; set; }
		public decimal ValorTotal { get; set; }
		public int QuantidadeItens { get; set; }
	}
}
