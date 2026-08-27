namespace Model.DTO.NFe
{
	/// <summary>
	/// Requisição de emissão de Carta de Correção Eletrônica (CC-e) para uma NF-e autorizada.
	/// </summary>
	public class CartaCorrecaoRequest
	{
		/// <summary>Id da NFeEmission (NF-e autorizada que receberá a correção).</summary>
		public int NFeId { get; set; }

		/// <summary>Texto da correção (xCorrecao). Mínimo 15 e máximo 1000 caracteres (MOC seção 5.10).</summary>
		public string TextoCorrecao { get; set; }
	}
}
