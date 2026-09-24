using Model;
using Model.DTO;
using Model.Registrations;
using Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service
{
	public class ClientService : BaseService<Client>, IClientService
	{
		public ClientService(IGenericRepository<Client> repository) : base(repository)
		{
		}

		public async Task<PagedResult<Client>> GetAllPaged(Filters clientFilter)
		{
			return await (repository as IClientRepository).GetAllPaged(clientFilter);
		}
		/// <summary>
		/// RN09/edicao — carrega um parceiro pelo Id para a tela de edicao.
		/// Retorna null quando nao existe (ou pertence a outra empresa), e o
		/// controller traduz isso em 404.
		/// </summary>
		public async Task<Client> GetById(int id, int idCompany)
		{
			return await (repository as IClientRepository).GetById(id, idCompany);
		}
		public async Task<List<Client>> GetByName(Filters clientFilter)
		{
			return await (repository as IClientRepository).GetByName(clientFilter);
		}
		public async Task<List<Client>> GetAllList(Filters clientFilter)
		{
			return await (repository as IClientRepository).GetAllList(clientFilter);
		}
		public async Task<ClientInfoResponse> GetByMonthAllClients(Filters filters)
		{
			return await (repository as IClientRepository).GetByMonthAllClients(filters);
		}
		public async Task<List<Client>> GetByFilter(Filters filter)
		{
			return await (repository as IClientRepository).GetByFilter(filter);
		}
		/// <summary>
		/// RN01/RN11 — já existe um cliente com este CPF/CNPJ nesta empresa?
		/// </summary>
		public async Task<bool> DocumentExists(int idCompany, string document, int? ignoreId = null)
		{
			return await (repository as IClientRepository)
				.DocumentExists(idCompany, document, ignoreId);
		}

		public async Task SaveClient(ClientDto model)
		{
			var client = new Client
			{
				Name = model.Name,
				IdCompany = model.IdCompany,
				TipoPessoa = string.IsNullOrEmpty(model.TipoPessoa) ? string.Empty : model.TipoPessoa,
				Document = string.IsNullOrEmpty(model.Document) ? string.Empty : model.Document,
				IndicadorIE = string.IsNullOrEmpty(model.IndicadorIE) ? string.Empty : model.IndicadorIE,
				ConsumidorFinal = string.IsNullOrEmpty(model.ConsumidorFinal) ? string.Empty : model.ConsumidorFinal,
				Address = string.IsNullOrEmpty(model.Address) ? string.Empty : model.Address,
				Numero = string.IsNullOrEmpty(model.Numero) ? string.Empty : model.Numero,
				Bairro = string.IsNullOrEmpty(model.Bairro) ? string.Empty : model.Bairro,
				Municipio = string.IsNullOrEmpty(model.Municipio) ? string.Empty : model.Municipio,
				CodMunicipioIbge = string.IsNullOrEmpty(model.CodMunicipioIbge) ? string.Empty : model.CodMunicipioIbge,
				Uf = string.IsNullOrEmpty(model.Uf) ? string.Empty : model.Uf,
				ZipCode = string.IsNullOrEmpty(model.ZipCode) ? string.Empty : model.ZipCode,
				Complemento = string.IsNullOrEmpty(model.Complemento) ? string.Empty : model.Complemento,
				Ie = string.IsNullOrEmpty(model.Ie) ? string.Empty : model.Ie,
				InscricaoMunicipal = string.IsNullOrEmpty(model.InscricaoMunicipal) ? string.Empty : model.InscricaoMunicipal,

				// RNTRC (usado pelo cadastro de veículos na RV10). Nulo quando não
				// informado — o parceiro comum não é transportador.
				Rntrc = string.IsNullOrWhiteSpace(model.Rntrc) ? null : model.Rntrc.Trim(),

				Email = string.IsNullOrEmpty(model.Email) ? string.Empty : model.Email,
				CellPhone = string.IsNullOrEmpty(model.CellPhone) ? string.Empty : model.CellPhone,
				Pais = string.IsNullOrEmpty(model.Pais) ? "Brasil" : model.Pais,
				CodPais = string.IsNullOrEmpty(model.CodPais) ? "1058" : model.CodPais,
				BirthDate = model.BirthDate,
				Status = model.Status,
				CreatDate = model.CreatDate,

				// Novos campos: sem este mapeamento eles seriam silenciosamente
				// descartados, porque este método monta o Client campo a campo.
				Profiles = model.Profiles,
				DriverLicense = MapDriverLicense(model.DriverLicense)
			};

			await base.Save(client);
		}

		/// <summary>
		/// Copia os dados de CNH do DTO para uma entidade nova.
		/// O vínculo (Id/IdClient) é resolvido pelo EF via navegação — por isso
		/// os identificadores vindos do payload são ignorados de propósito.
		/// </summary>
		private static DriverLicense? MapDriverLicense(DriverLicense? source)
		{
			if (source == null)
			{
				return null;
			}

			return new DriverLicense
			{
				NumeroCnh = source.NumeroCnh,
				CategoriaCnh = source.CategoriaCnh,
				DataEmissaoCnh = source.DataEmissaoCnh,
				DataValidadeCnh = source.DataValidadeCnh,
				PrimeiraHabilitacao = source.PrimeiraHabilitacao,
				UfEmissaoCnh = source.UfEmissaoCnh,
				PossuiEar = source.PossuiEar,
				ObservacoesCnh = source.ObservacoesCnh
			};
		}
	}
	public interface IClientService : IBaseService<Client>
	{
		Task<PagedResult<Client>> GetAllPaged(Filters clientFilter);
		Task<List<Client>> GetByName(Filters clientFilter);
		Task<List<Client>> GetAllList(Filters clientFilter);
		Task<ClientInfoResponse> GetByMonthAllClients(Filters filters);
		Task<Client> GetById(int id, int idCompany);
		Task<List<Client>> GetByFilter(Filters filter);
		Task<bool> DocumentExists(int idCompany, string document, int? ignoreId = null);
		Task SaveClient(ClientDto model);
	}
}
