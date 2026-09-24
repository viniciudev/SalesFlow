using Model.Moves;
using Model.Registrations;
using System;
using Model.Enums;

namespace Model
{
	public class Filters
	{
		public string? TextOption { get; set; }
		public FilterType SelectOption { get; set; }
		public string? cellPhoneOption { get; set; }
		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public int CodGroup { get; set; }
		public int IdSale { get; set; }
		public int IdSalesman { get; set; }

		public int IdCompany { get; set; }
		public int IdBudget { get; set; }
		public int IdServiceProvision { get; set; }
		public int IdClient { get; set; }
		public DateTime SaleDate { get; set; }
		public DateTime SaleDateFinal { get; set; }
		public DateTime CheckinDate { get; set; }
		public DateTime CheckinDateFinal { get; set; }
		public int IdSeller { get; set; }
		public FinancialType? FinancialType { get; set; }
		public FinancialStatus? FinancialStatus { get; set; }
		public StatusNfe? StatusNfe { get; set; }

		// Status da OS e da NFS-e. NÃO reutilizar StatusNfe para isto: ele colide
		// numericamente com os dois enums abaixo (StatusNfe.pendente=1 bate com
		// ServiceOrderStatus.Aberta=1), e o filtro passaria a casar pelo valor errado.
		public ServiceOrderStatus? ServiceOrderStatus { get; set; }
		public ServiceInvoiceStatus? ServiceInvoiceStatus { get; set; }
		public TipoDocumentoEnum ?TipoDocumento { get; set; }
		public string? StartDate { get; set; }      // Formato: "yyyy-MM-dd"
		public string? EndDate { get; set; }        // Formato: "yyyy-MM-dd"
		public int? ClientId { get; set; }
		public int? PaymentMethodId { get; set; }
		public int? BankAccountId { get; set; }
		public bool? SalesOrder { get; set; }
		public SaleStatus? SaleStatus { get; set; }
		public string? Search { get; set; }

		// ===== Cadastro de veículos (MDF-e) =====
		// A placa NÃO tem campo próprio de propósito: ela é buscada por
		// TextOption, como o nome do produto/cliente — o repositório procura em
		// placa E código interno com um único parâmetro.
		//
		// Tipo anulável de propósito: null = "todos". Um valor não anulável
		// tornaria impossível distinguir "não filtrar" de "filtrar por Traction".
		public string? LicensingState { get; set; }
		public VehicleType? VehicleType { get; set; }

		/// <summary>
		/// null = todos (ativos e inativos). true = só ativos, false = só
		/// inativos. Note que a listagem do cadastro mostra os dois, então o
		/// padrão do repositório é não filtrar por aqui.
		/// </summary>
		public bool? VehicleIsActive { get; set; }
	}

	public enum FilterType
	{
		Name,
		Cpf,
	}

}
