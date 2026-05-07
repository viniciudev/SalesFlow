using Model;
using Model.DTO;
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
				Email = string.IsNullOrEmpty(model.Email) ? string.Empty : model.Email,
				CellPhone = string.IsNullOrEmpty(model.CellPhone) ? string.Empty : model.CellPhone,
				Pais = string.IsNullOrEmpty(model.Pais) ? "Brasil" : model.Pais,
				CodPais = string.IsNullOrEmpty(model.CodPais) ? "1058" : model.CodPais,
				BirthDate = model.BirthDate,
				Status = model.Status,
				CreatDate = model.CreatDate
			};

			await base.Save(client);
		}
	}
	public interface IClientService : IBaseService<Client>
	{
		Task<PagedResult<Client>> GetAllPaged(Filters clientFilter);
		Task<List<Client>> GetByName(Filters clientFilter);
		Task<List<Client>> GetAllList(Filters clientFilter);
		Task<ClientInfoResponse> GetByMonthAllClients(Filters filters);
		Task<List<Client>> GetByFilter(Filters filter);
		Task SaveClient(ClientDto model);
	}
}
