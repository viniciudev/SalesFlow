using Model;
using Model.Enums;
using Model.Registrations;
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

	/// <summary>
	/// RNTRC do transportador (ANTT). Exigido pela RV10 do cadastro de veículos
	/// quando o parceiro é proprietário de um veículo que não é próprio.
	///
	/// 8 dígitos, como o tipo <c>TRNTRC</c> dos XSDs do MDF-e
	/// (<c>&lt;xs:pattern value="[0-9]{8}"/&gt;</c>) e a coluna
	/// <c>tb_client.Rntrc</c>. Mesmo limite da entidade
	/// <c>Client.Rntrc</c> — se divergirem, o POST e o PUT passam a aceitar
	/// coisas diferentes.
	/// </summary>
	[StringLength(8)]
	public string? Rntrc { get; set; }
	public string? Email { get; set; }
	public string CellPhone { get; set; } = string.Empty;
	public string Pais { get; set; } = "Brasil";
	public string CodPais { get; set; } = "1058";
	public DateTime? BirthDate { get; set; }
	public Client.statusType Status { get; set; } = Client.statusType.Ativo;
	public DateTime CreatDate { get; set; } = DateTime.Now;

	/// <summary>
	/// Classificações do parceiro. Serializado como número (bitmask) — o valor
	/// padrão "Cliente" (1) preserva o comportamento anterior para payloads
	/// antigos que ainda não enviam o campo.
	/// </summary>
	public PartnerProfile Profiles { get; set; } = PartnerProfile.Cliente;

	/// <summary>
	/// Dados de CNH. Preenchido apenas quando Profiles inclui Motorista.
	/// No POST o Id/IdClient são ignorados — o vínculo é feito com o cliente
	/// recém-criado em <c>ClientService.SaveClient</c>.
	/// </summary>
	public DriverLicense? DriverLicense { get; set; }
}