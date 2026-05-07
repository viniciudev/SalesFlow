using Model;
using System;
using System.ComponentModel.DataAnnotations;

public class ClientDto
{
	[Required(ErrorMessage = "O nome é obrigatório")]
	[StringLength(60)]
	public string Name { get; set; }

	// Campos opcionais
	public int IdCompany { get; set; }
	public string TipoPessoa { get; set; } = string.Empty;
	public string Document { get; set; } = string.Empty;
	public string IndicadorIE { get; set; } = string.Empty;
	public string ConsumidorFinal { get; set; } = string.Empty;
	public string Address { get; set; } = string.Empty;
	public string Numero { get; set; } = string.Empty;
	public string Bairro { get; set; } = string.Empty;
	public string Municipio { get; set; } = string.Empty;
	public string CodMunicipioIbge { get; set; } = string.Empty;
	public string Uf { get; set; } = string.Empty;
	public string ZipCode { get; set; } = string.Empty;
	public string Complemento { get; set; } = string.Empty;
	public string Ie { get; set; } = string.Empty;
	public string InscricaoMunicipal { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string CellPhone { get; set; } = string.Empty;
	public string Pais { get; set; } = "Brasil";
	public string CodPais { get; set; } = "1058";
	public DateTime? BirthDate { get; set; }
	public Client.statusType Status { get; set; } = Client.statusType.Ativo;
	public DateTime CreatDate { get; set; } = DateTime.Now;
}