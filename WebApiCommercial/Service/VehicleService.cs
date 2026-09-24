using Model;
using Model.DTO;
using Model.Enums;
using Model.Registrations;
using Repository;
using Service.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service
{
	/// <summary>
	/// Regras do cadastro de veículos (MDF-e) — RV01 a RV14 da OS-002.
	///
	/// Por que as regras moram aqui e não em atributo no DTO: as que dependem de
	/// banco (RV02 unicidade de placa, RV10 dados do parceiro) ou de outra
	/// entidade não têm como ser expressas em DataAnnotations. O DTO cobre
	/// presença/formato/faixa — o que o <c>[ApiController]</c> já converte em 400
	/// sozinho — e este serviço cobre o resto, lançando <see cref="DomainException"/>
	/// com TODAS as pendências de uma vez (não só a primeira), para o usuário
	/// corrigir o formulário em uma passada.
	///
	/// RV13 (não criar FK fixa motorista × veículo) não tem código aqui de
	/// propósito: é uma regra de "não fazer". O rastro da associação vai para
	/// <see cref="VehicleUsageHistory"/> (RV14), que nasce vazia nesta entrega.
	/// </summary>
	public class VehicleService : BaseService<Vehicle>, IVehicleService
	{
		private readonly IClientRepository _clientRepository;

		public VehicleService(IGenericRepository<Vehicle> repository, IClientRepository clientRepository)
			: base(repository)
		{
			_clientRepository = clientRepository;
		}

		/// <summary>
		/// Atalho para a interface específica. O <c>BaseService</c> guarda o
		/// repositório como <c>IGenericRepository</c>, que não expõe os métodos
		/// do veículo — mesmo padrão do <c>ClientService</c>.
		/// </summary>
		private IVehicleRepository Repo => repository as IVehicleRepository;

		// =====================================================================
		// CRUD
		// =====================================================================

		/// <summary>Cria um veículo. Valida RV01–RV10 antes de gravar.</summary>
		public async Task<VehicleResponseDto> CreateAsync(VehicleCreateDto dto, int idCompany)
		{
			await ValidateAsync(dto, idCompany, ignoreId: null);

			var vehicle = new Vehicle { IdCompany = idCompany, IsActive = true };
			ApplyDto(vehicle, dto, idCompany);

			await Repo.AddAsync(vehicle);

			// Relê para trazer o Client já materializado: o veículo que acabou de
			// ser gravado tem só o ClientId preenchido, e o DTO de resposta
			// precisa do nome/documento do proprietário para a tela não exibir o
			// parceiro em branco logo após salvar.
			var saved = await Repo.GetByIdAsync(vehicle.Id, idCompany);
			return MapToResponse(saved ?? vehicle);
		}

		/// <summary>
		/// Atualiza um veículo. RV02 é reavaliada ignorando o próprio Id, senão
		/// salvar o formulário sem trocar a placa acusaria conflito consigo mesmo.
		/// </summary>
		public async Task<VehicleResponseDto> UpdateAsync(int id, VehicleUpdateDto dto, int idCompany)
		{
			var vehicle = await Repo.GetByIdAsync(id, idCompany);
			if (vehicle == null)
				throw new DomainException("Veículo não encontrado.");

			await ValidateAsync(dto, idCompany, ignoreId: id);

			ApplyDto(vehicle, dto, idCompany);
			vehicle.IsActive = dto.IsActive;

			await Repo.UpdateAsync(vehicle);

			var updated = await Repo.GetByIdAsync(id, idCompany);
			return MapToResponse(updated ?? vehicle);
		}

		/// <summary>Detalhe do veículo, restrito à empresa.</summary>
		public async Task<VehicleResponseDto> GetByIdAsync(int id, int idCompany)
		{
			var vehicle = await Repo.GetByIdAsync(id, idCompany);
			if (vehicle == null)
				throw new DomainException("Veículo não encontrado.");

			return MapToResponse(vehicle);
		}

		/// <summary>
		/// Listagem paginada. Devolve ativos E inativos — a tela do cadastro
		/// mostra os dois, com a coluna "Ativo" e o filtro correspondente.
		/// </summary>
		public async Task<PagedResult<VehicleListItemDto>> GetPagedAsync(Filters filter)
		{
			var paged = await Repo.GetPagedAsync(filter);

			// Copia os metadados de paginação em vez de herdar PagedResult<Vehicle>:
			// PagedResult<T> é genérico mas não covariante (é classe), então não há
			// como reusar a instância — e mapear para o DTO aqui evita mandar a
			// entidade inteira (com Company e UsageHistory) para o front-end.
			return new PagedResult<VehicleListItemDto>
			{
				CurrentPage = paged.CurrentPage,
				PageCount = paged.PageCount,
				PageSize = paged.PageSize,
				RowCount = paged.RowCount,
				Results = paged.Results.Select(MapToListItem).ToList()
			};
		}

		/// <summary>
		/// RV11 — exclusão lógica. Nunca há DELETE físico: o veículo sai da
		/// seleção de MDF-e (RV12) e continua na listagem, marcado como inativo.
		/// Isso também libera a placa para recadastro, porque o índice único é
		/// parcial (só entre ativos).
		/// </summary>
		public async Task DeactivateAsync(int id, int idCompany)
		{
			await SetActiveAsync(id, idCompany, false, "desativar");
		}

		/// <summary>Reativa um veículo previamente desativado (RV11).</summary>
		public async Task ActivateAsync(int id, int idCompany)
		{
			await SetActiveAsync(id, idCompany, true, "reativar");
		}

		/// <summary>
		/// RV12 — somente veículos ativos, para os seletores de MDF-e.
		///
		/// Não é a listagem do cadastro: aqui o filtro de ativo é obrigatório,
		/// porque um veículo desativado oferecido ao manifesto vira rejeição da
		/// SEFAZ. Cada item já vem com o checklist resolvido, para o seletor
		/// poder desabilitar (em vez de esconder) o que ainda tem pendência.
		/// </summary>
		public async Task<IEnumerable<VehicleListItemDto>> GetAvailableForMdfEAsync(int idCompany)
		{
			var vehicles = await Repo.GetActiveForMdfEAsync(idCompany);
			return vehicles.Select(MapToListItem).ToList();
		}

		/// <summary>Status de aptidão de um veículo já cadastrado.</summary>
		public async Task<MdfEStatusResult> GetMdfEStatusAsync(int id, int idCompany)
		{
			var vehicle = await Repo.GetByIdAsync(id, idCompany);
			if (vehicle == null)
				throw new DomainException("Veículo não encontrado.");

			return CalculateMdfEStatus(vehicle);
		}

		// =====================================================================
		// Checklist "Apto para MDF-e"
		// =====================================================================

		/// <summary>
		/// Checklist de aptidão para o MDF-e — os 6 itens definidos na OS-002.
		///
		/// Devolve TODAS as pendências, não só a primeira: o objetivo é o usuário
		/// corrigir o cadastro em uma passada, em vez de descobrir um item por
		/// tentativa a cada tentativa de emissão. <see cref="MdfEStatusResult.IsReady"/>
		/// é sempre <c>PendingItems.Count == 0</c>.
		///
		/// É método de instância (e não estático) para poder ser sobrescrito e
		/// para ficar no mesmo lugar das RV que ele resume; não usa estado, então
		/// é testável isoladamente.
		///
		/// Note que o checklist NÃO inclui RENAVAM (RV03, exigido na validação) nem
		/// a situação ativa/inativa: a OS lista exatamente estes 6 itens, e a
		/// inatividade já é barrada antes, no filtro de <see cref="GetAvailableForMdfEAsync"/>.
		/// </summary>
		public MdfEStatusResult CalculateMdfEStatus(Vehicle vehicle)
		{
			var pending = new List<string>();

			if (vehicle == null)
			{
				return new MdfEStatusResult
				{
					IsReady = false,
					PendingItems = new List<string> { "Veículo não encontrado." }
				};
			}

			// 1. Placa informada e válida (Mercosul ou antiga).
			if (!VehiclePlate.IsValid(vehicle.LicensePlate))
				pending.Add("Placa não informada ou em formato inválido (use ABC1234 ou ABC1D23).");

			// 2. UF de licenciamento preenchida — e existente, senão a SEFAZ
			//    rejeita o veicTracao/veicReboque por UF inválida.
			if (!UfList.IsValid(vehicle.LicensingState))
				pending.Add("UF de licenciamento não informada ou inválida.");

			// 3. Tipo de rodado — domínio fechado do MOC.
			if (!Enum.IsDefined(typeof(WheelType), vehicle.WheelType))
				pending.Add("Tipo de rodado não selecionado.");

			// 4. Tipo de carroceria — domínio fechado do MOC.
			//    BodyType.NaoAplicavel (0) é valor legítimo, então "definido" aqui
			//    é "está no domínio", não "é diferente de zero".
			if (!Enum.IsDefined(typeof(BodyType), vehicle.BodyType))
				pending.Add("Tipo de carroceria não selecionado.");

			// 5. Tara maior que zero — a SEFAZ rejeita tara zerada.
			if (vehicle.Tare <= 0)
				pending.Add("Tara (KG) não informada ou igual a zero.");

			// 6. Se terceiro: dados do proprietário (CPF/CNPJ e RNTRC).
			//    Só se aplica quando o veículo não é próprio — para veículo
			//    próprio o proprietário é a própria empresa e não há o que exigir.
			if (vehicle.OwnershipType != OwnershipType.Proprio)
			{
				if (vehicle.Client == null)
				{
					pending.Add("Veículo de terceiro sem proprietário vinculado.");
				}
				else
				{
					if (string.IsNullOrWhiteSpace(vehicle.Client.Document))
						pending.Add("Proprietário sem CPF/CNPJ informado.");

					if (string.IsNullOrWhiteSpace(vehicle.Client.Rntrc))
						pending.Add("Proprietário sem RNTRC informado.");
				}
			}

			return new MdfEStatusResult
			{
				IsReady = pending.Count == 0,
				PendingItems = pending
			};
		}

		// =====================================================================
		// Validação (RV01–RV10)
		// =====================================================================

		/// <summary>
		/// Valida o payload e lança <see cref="DomainException"/> com todas as
		/// pendências juntas.
		///
		/// Acumular em vez de falhar na primeira é deliberado: cada mensagem
		/// corresponde a um campo do formulário, e devolver uma por vez forçaria
		/// uma rodada de salvar-corrigir por campo.
		/// </summary>
		private async Task ValidateAsync(VehicleCreateDto dto, int idCompany, int? ignoreId)
		{
			var errors = new List<string>();

			// RV01 — placa obrigatória e em formato válido.
			// A checagem de vazio repete o [Required] do DTO de propósito: o
			// serviço também é chamado fora do pipeline do [ApiController].
			if (string.IsNullOrWhiteSpace(dto.LicensePlate))
			{
				errors.Add("A placa é obrigatória.");
			}
			else if (!VehiclePlate.IsValid(dto.LicensePlate))
			{
				errors.Add("Placa inválida. Use o formato Mercosul (ABC1D23) ou o antigo (ABC1234).");
			}
			else
			{
				// RV02 — a placa só precisa ser única entre veículos ATIVOS.
				// Um veículo desativado com a mesma placa é permitido (é o que
				// permite recadastrar a placa depois de desativar o antigo), e
				// por isso a checagem é sobre "ativo", não sobre "existe".
				var plateTaken = await Repo.ExistsActivePlateAsync(idCompany, dto.LicensePlate, ignoreId);
				if (plateTaken)
					errors.Add($"Já existe um veículo ativo com a placa {VehiclePlate.Normalize(dto.LicensePlate)}.");
			}

			// RV03 — RENAVAM, quando informado, precisa ter 11 dígitos.
			if (!VehiclePlate.IsValidRenavam(dto.Renavam))
				errors.Add("O RENAVAM deve ter exatamente 11 dígitos.");

			// RV07 — UF precisa existir, não só ter 2 caracteres.
			if (!UfList.IsValid(dto.LicensingState))
				errors.Add("UF de licenciamento inválida. Informe uma das 27 siglas (ex.: SP).");

			// RV04 — tara maior que zero. O [Range(1, ...)] do DTO cobre o mesmo,
			// mas aqui a mensagem sai junto das outras pendências.
			if (dto.Tare <= 0)
				errors.Add("A tara (KG) é obrigatória e deve ser maior que zero.");

			// RV05/RV06/RV08 — enums obrigatórios e dentro do domínio do MOC.
			// Enum.IsDefined pega valor fora da faixa (ex.: veio 99 no JSON), que
			// o [Required] sozinho não pega — para o tipo não anulável, 99 é
			// "informado".
			if (!dto.VehicleType.HasValue || !Enum.IsDefined(typeof(VehicleType), dto.VehicleType.Value))
				errors.Add("O tipo de veículo é obrigatório (Tração ou Reboque).");

			if (!dto.WheelType.HasValue || !Enum.IsDefined(typeof(WheelType), dto.WheelType.Value))
				errors.Add("O tipo de rodado é obrigatório.");

			if (!dto.BodyType.HasValue || !Enum.IsDefined(typeof(BodyType), dto.BodyType.Value))
				errors.Add("O tipo de carroceria é obrigatório.");

			if (!dto.OwnershipType.HasValue || !Enum.IsDefined(typeof(OwnershipType), dto.OwnershipType.Value))
				errors.Add("O tipo de propriedade é obrigatório.");

			if (dto.CapacityKg.HasValue && dto.CapacityKg.Value < 0)
				errors.Add("A capacidade (KG) não pode ser negativa.");

			if (dto.CapacityM3.HasValue && dto.CapacityM3.Value < 0)
				errors.Add("A capacidade (M³) não pode ser negativa.");

			// RV09/RV10 — proprietário terceiro.
			// Só roda quando o enum é válido: sem isso, um OwnershipType inválido
			// cairia no ramo "!= Proprio" e geraria uma segunda mensagem sobre o
			// proprietário, confundindo a causa raiz.
			if (dto.OwnershipType.HasValue
				&& Enum.IsDefined(typeof(OwnershipType), dto.OwnershipType.Value)
				&& dto.OwnershipType.Value != OwnershipType.Proprio)
			{
				await ValidateThirdPartyOwnerAsync(dto, idCompany, errors);
			}

			if (errors.Count > 0)
				throw new DomainException(string.Join(" ", errors));
		}

		/// <summary>
		/// RV09/RV10 — veículo que não é próprio exige o parceiro proprietário
		/// com os dados que o MDF-e cobra: CPF/CNPJ (<c>Document</c>), RNTRC
		/// (exigido do transportador TAC) e inscrição estadual.
		///
		/// A mensagem cita o campo que falta, um a um, porque "dados do
		/// proprietário incompletos" obrigaria o usuário a adivinhar o que
		/// preencher — e o cadastro do parceiro é outra tela.
		/// </summary>
		private async Task ValidateThirdPartyOwnerAsync(VehicleCreateDto dto, int idCompany, List<string> errors)
		{
			if (!dto.ClientId.HasValue || dto.ClientId.Value <= 0)
			{
				errors.Add("Veículo não próprio exige um proprietário (parceiro) vinculado.");
				return;
			}

			// GetById do ClientRepository já filtra por empresa: um ClientId de
			// outra empresa volta null e cai como "não encontrado", em vez de
			// vazar o parceiro alheio para o cadastro.
			var owner = await _clientRepository.GetById(dto.ClientId.Value, idCompany);
			if (owner == null)
			{
				errors.Add("Proprietário não encontrado nesta empresa.");
				return;
			}

			if (string.IsNullOrWhiteSpace(owner.Document))
				errors.Add("O proprietário do veículo está sem CPF/CNPJ. Complete o cadastro do parceiro.");

			if (string.IsNullOrWhiteSpace(owner.Rntrc))
				errors.Add("O proprietário do veículo está sem RNTRC (ANTT). Complete o cadastro do parceiro.");

			if (string.IsNullOrWhiteSpace(owner.Ie))
				errors.Add("O proprietário do veículo está sem Inscrição Estadual. Complete o cadastro do parceiro.");

			if (!UfList.IsValid(owner.Uf))
				errors.Add("O proprietário do veículo está sem UF válida. Complete o cadastro do parceiro.");
		}

		// =====================================================================
		// Mapeamento
		// =====================================================================

		/// <summary>
		/// Copia o DTO para a entidade.
		///
		/// <c>IdCompany</c> é parâmetro separado, e não lido do DTO, de
		/// propósito: a empresa vem do header da requisição (tenant), nunca do
		/// corpo — aceitar do corpo permitiria gravar veículo em empresa alheia.
		/// </summary>
		private void ApplyDto(Vehicle vehicle, VehicleCreateDto dto, int idCompany)
		{
			vehicle.IdCompany = idCompany;
			vehicle.InternalCode = string.IsNullOrWhiteSpace(dto.InternalCode) ? null : dto.InternalCode.Trim();
			vehicle.LicensePlate = VehiclePlate.Normalize(dto.LicensePlate);
			vehicle.Renavam = VehiclePlate.NormalizeRenavam(dto.Renavam);
			vehicle.LicensingState = UfList.Normalize(dto.LicensingState) ?? dto.LicensingState.Trim().ToUpper();
			vehicle.VehicleType = dto.VehicleType.Value;
			vehicle.WheelType = dto.WheelType.Value;
			vehicle.BodyType = dto.BodyType.Value;
			vehicle.Tare = dto.Tare;
			vehicle.CapacityKg = dto.CapacityKg;
			vehicle.CapacityM3 = dto.CapacityM3;
			vehicle.OwnershipType = dto.OwnershipType.Value;

			var isThirdParty = dto.OwnershipType.Value != OwnershipType.Proprio;

			// Veículo próprio não guarda proprietário nem categoria de terceiro.
			// Zerar aqui — em vez de só ignorar — impede que trocar o tipo de
			// "Terceiro" para "Próprio" deixe um parceiro órfão apontado na
			// coluna, que o checklist de RV10 depois acusaria sem motivo.
			vehicle.ClientId = isThirdParty ? dto.ClientId : null;

			// ThirdPartyCategory só existe no MOC para proprietário terceiro;
			// a validação de enum é feita só quando o valor vem preenchido.
			vehicle.ThirdPartyCategory = isThirdParty
				&& dto.ThirdPartyCategory.HasValue
				&& Enum.IsDefined(typeof(ThirdPartyCategory), dto.ThirdPartyCategory.Value)
					? dto.ThirdPartyCategory
					: null;
		}

		private VehicleResponseDto MapToResponse(Vehicle vehicle)
		{
			return new VehicleResponseDto
			{
				Id = vehicle.Id,
				IdCompany = vehicle.IdCompany,
				InternalCode = vehicle.InternalCode,
				LicensePlate = vehicle.LicensePlate,
				Renavam = vehicle.Renavam,
				LicensingState = vehicle.LicensingState,
				VehicleType = vehicle.VehicleType,
				WheelType = vehicle.WheelType,
				BodyType = vehicle.BodyType,
				Tare = vehicle.Tare,
				CapacityKg = vehicle.CapacityKg,
				CapacityM3 = vehicle.CapacityM3,
				OwnershipType = vehicle.OwnershipType,
				ThirdPartyCategory = vehicle.ThirdPartyCategory,
				Owner = MapOwner(vehicle),
				IsActive = vehicle.IsActive,
				CreatedAt = vehicle.CreatedAt,
				UpdatedAt = vehicle.UpdatedAt,
				MdfEStatus = CalculateMdfEStatus(vehicle)
			};
		}

		/// <summary>
		/// Achata o parceiro proprietário. Devolve null para veículo próprio —
		/// não há proprietário externo a exibir, e um objeto vazio faria a tela
		/// mostrar uma seção de proprietário em branco.
		/// </summary>
		private static VehicleOwnerDto MapOwner(Vehicle vehicle)
		{
			if (vehicle.OwnershipType == OwnershipType.Proprio || vehicle.Client == null)
				return null;

			return new VehicleOwnerDto
			{
				ClientId = vehicle.Client.Id,
				Name = vehicle.Client.Name,
				Document = vehicle.Client.Document,
				TipoPessoa = vehicle.Client.TipoPessoa,
				Ie = vehicle.Client.Ie,
				Uf = vehicle.Client.Uf,
				Rntrc = vehicle.Client.Rntrc
			};
		}

		private VehicleListItemDto MapToListItem(Vehicle vehicle)
		{
			var status = CalculateMdfEStatus(vehicle);

			return new VehicleListItemDto
			{
				Id = vehicle.Id,
				InternalCode = vehicle.InternalCode,
				LicensePlate = vehicle.LicensePlate,
				LicensingState = vehicle.LicensingState,
				VehicleType = vehicle.VehicleType,
				WheelType = vehicle.WheelType,
				BodyType = vehicle.BodyType,
				Tare = vehicle.Tare,
				CapacityKg = vehicle.CapacityKg,
				CapacityM3 = vehicle.CapacityM3,
				OwnershipType = vehicle.OwnershipType,
				ClientId = vehicle.ClientId,
				ClientName = vehicle.Client?.Name,
				IsActive = vehicle.IsActive,
				IsReadyForMdfE = status.IsReady,
				MdfEPendingItems = status.PendingItems
			};
		}

		/// <summary>
		/// Caminho comum de desativar/reativar. Usa <c>SetActiveAsync</c> do
		/// repositório (UPDATE direto) em vez de carregar-e-gravar: assim o
		/// parceiro proprietário não é regravado junto, e a operação é idempotente
		/// — reativar um veículo já ativo não é erro.
		/// </summary>
		private async Task SetActiveAsync(int id, int idCompany, bool isActive, string action)
		{
			var affected = await Repo.SetActiveAsync(id, idCompany, isActive);

			if (affected == 0)
				throw new DomainException($"Veículo não encontrado, não foi possível {action}.");
		}
	}

	/// <summary>
	/// Contrato do cadastro de veículos.
	///
	/// Atenção: <c>IBaseService&lt;Vehicle&gt;</c> também expõe
	/// <c>GetByIdAsync(int)</c>, que NÃO filtra empresa (vem do
	/// <c>GenericRepository</c>). Use sempre a sobrecarga
	/// <c>GetByIdAsync(int, int)</c> desta interface em código de API.
	/// </summary>
	public interface IVehicleService : IBaseService<Vehicle>
	{
		Task<VehicleResponseDto> CreateAsync(VehicleCreateDto dto, int idCompany);
		Task<VehicleResponseDto> UpdateAsync(int id, VehicleUpdateDto dto, int idCompany);
		Task<VehicleResponseDto> GetByIdAsync(int id, int idCompany);
		Task<PagedResult<VehicleListItemDto>> GetPagedAsync(Filters filter);
		Task DeactivateAsync(int id, int idCompany);
		Task ActivateAsync(int id, int idCompany);
		Task<MdfEStatusResult> GetMdfEStatusAsync(int id, int idCompany);
		Task<IEnumerable<VehicleListItemDto>> GetAvailableForMdfEAsync(int idCompany);
		MdfEStatusResult CalculateMdfEStatus(Vehicle vehicle);
	}
}
