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

		// ===== MDF-e =====
		// Mesma convenção do bloco acima: anulável para "não filtrar" ser
		// distinguível de um valor legítimo (MdfeStatus.Rascunho é 1, e
		// MdfeTipoEmitente.Pst também — com tipo não anulável, "todos" viraria
		// "só PST").
		//
		// NÃO reutilizar StatusNfe aqui: StatusNfe.pendente=1 colide com
		// MdfeStatus.Rascunho=1 e com MdfeTipoEmitente.Pst=1, e o filtro
		// passaria a casar pelo valor errado — o mesmo motivo já registrado no
		// bloco de OS/NFS-e acima.
		public MdfeStatus? MdfeStatus { get; set; }
		public MdfeTipoEmitente? MdfeTipoEmitente { get; set; }
		public MdfeTipoOperacao? MdfeTipoOperacao { get; set; }
		public string? MdfeUfCarregamento { get; set; }
		public string? MdfeUfDescarregamento { get; set; }

		/// <summary>
		/// Chave de acesso ou número do manifesto. Buscado por TextOption, como a
		/// placa no cadastro de veículos — o repositório casa os dois com um
		/// parâmetro só.
		/// </summary>
		public string? MdfeChave { get; set; }

		// ===== Pesquisa de documentos para manifestar (aba Documentos) =====
		// Filtros da área vermelha da tela. Separados dos do manifesto porque
		// respondem a outra pergunta: "quais NOTAS podem entrar num manifesto",
		// e não "quais manifestos existem".
		//
		// Entrada (Purchase) e Saída (NFeEmission) na mesma pesquisa: nulo = as
		// duas origens.
		public MdfeTipoDocumento? DocumentoTipo { get; set; }
		public string? DocumentoUfOrigem { get; set; }
		public string? DocumentoUfDestino { get; set; }

		/// <summary>Id do parceiro (cliente na saída, fornecedor na entrada).</summary>
		public int? DocumentoParceiroId { get; set; }

		/// <summary>Chave de acesso da NF-e, parcial ou completa.</summary>
		public string? DocumentoChave { get; set; }

		/// <summary>Número da NF-e.</summary>
		public long? DocumentoNumero { get; set; }

		/// <summary>
		/// Id do manifesto sendo editado, para a elegibilidade (RM10) não acusar
		/// como "já vinculado" os documentos que já são DESTE manifesto — senão a
		/// tela de edição não conseguiria listar o que ela própria já tem.
		/// </summary>
		public int? DocumentoIgnorarMdfeId { get; set; }
	}

	public enum FilterType
	{
		Name,
		Cpf,
	}

}
