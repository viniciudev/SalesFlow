#nullable enable
using Model.Enums;
using Model.Moves;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Registrations
{
	public class NFeEmission : BaseEntity
	{
		public string? ChaveAcesso { get; set; }

		public int NaturezaOperacaoId { get; set; }
		public NaturezaOperacao? NaturezaOperacao { get; set; }

		// Venda relacionada (opcional)
		public int SaleId { get; set; }
		public Sale? Sale { get; set; }

		// Dados de controle / retry
		public TipoDocumentoEnum TipoDocumento { get; set; } = TipoDocumentoEnum.NFE; // NFE ou NFCE
		public string? Serie { get; set; }
		public long? Numero { get; set; } // n�mero emitido pela SEFAZ quando sucesso

		public bool Sent { get; set; } = false; // true quando emitido com sucesso
		public int TryCount { get; set; } = 0;

		// Payload e resposta (JSON armazenado para reprocessar / debug)
		[Column(TypeName = "text")]
		public string? RequestPayloadJson { get; set; }
		[Column(TypeName = "text")]
		public string? ResponseJson { get; set; }
		[Column(TypeName = "text")]
		public string? ErrorMessage { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
		public Company? Company { get; set; }
		public int CompanyId { get; set; }
		public StatusNfe StatusNfe { get; set; }
		[Column(TypeName = "text")] // Usar text para armazenar o XML completo
		public string? XmlCompleto { get; set; }
		public string? Protocolo { get; set; }
		public string? MotivoCancelamento { get; set; }

		/// <summary>
		/// JSON com resumo da origem da tributacao usada por cada item da nota.
		/// Ex: [{"nItem":1,"origem":"Produto","naturezaOrigemId":5}, {"nItem":2,"origem":"NaturezaOperacao"}]
		/// </summary>
		[Column(TypeName = "text")]
		public string? TributacaoAuditJson { get; set; }
	}
	public enum StatusNfe
	{
		pendente = 1,
		emitida = 2,
		cancelada = 3,
		outros = 4
	}
}