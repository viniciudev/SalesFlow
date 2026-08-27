#nullable enable
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Registrations
{
	/// <summary>
	/// Evento fiscal vinculado a uma NF-e (ex.: Carta de Correção Eletrônica - CC-e, tpEvento 110110).
	/// Persiste o evento transmitido à SEFAZ (XML assinado / procEventoNFe), o protocolo e o retorno.
	/// </summary>
	public class NFeEvento : BaseEntity
	{
		public int NFeEmissionId { get; set; }
		public NFeEmission? NFeEmission { get; set; }

		public int CompanyId { get; set; }
		public Company? Company { get; set; }

		/// <summary>Código do tipo do evento (110110 = Carta de Correção).</summary>
		public int TipoEvento { get; set; }

		/// <summary>Descrição do evento (ex.: "Carta de Correção").</summary>
		public string? DescricaoEvento { get; set; }

		/// <summary>Sequencial do evento para o mesmo tipo (nSeqEvento).</summary>
		public int NSeqEvento { get; set; }

		/// <summary>Chave de acesso da NF-e vinculada ao evento (chNFe).</summary>
		public string? ChaveAcesso { get; set; }

		/// <summary>Texto da correção (xCorrecao) - Carta de Correção.</summary>
		[Column(TypeName = "text")]
		public string? Correcao { get; set; }

		/// <summary>Número do protocolo do evento (nProt).</summary>
		public string? Protocolo { get; set; }

		/// <summary>Código de status do retorno da SEFAZ (cStat).</summary>
		public int? CStat { get; set; }

		/// <summary>Descrição do status do retorno da SEFAZ (xMotivo).</summary>
		[Column(TypeName = "text")]
		public string? XMotivo { get; set; }

		/// <summary>Data/hora de registro do evento na SEFAZ (dhRegEvento).</summary>
		public DateTime? DhRegEvento { get; set; }

		/// <summary>XML do evento processado (procEventoNFe) - evento assinado + retorno da SEFAZ.</summary>
		[Column(TypeName = "text")]
		public string? XmlEvento { get; set; }

		/// <summary>Situação do evento no sistema (autorizado, rejeitado ou falha de comunicação).</summary>
		public SituacaoEvento Situacao { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}

	public enum SituacaoEvento
	{
		Autorizado = 1,
		Rejeitado = 2,
		FalhaComunicacao = 3
	}
}
