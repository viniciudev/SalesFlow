using Model.Registrations;
using Repository;
using System.Threading.Tasks;
using WebApiCommercial.Dtos;

namespace Service
{
	public class FiscalConfigurationService : BaseService<FiscalConfiguration>, IFiscalConfigurationService
	{
		private readonly IFiscalConfigurationRepository _fiscalRepository;

		public FiscalConfigurationService(IGenericRepository<FiscalConfiguration> repository,
																			IFiscalConfigurationRepository fiscalRepository) : base(repository)
		{
			_fiscalRepository = fiscalRepository;
		}

		public async Task<FiscalConfiguration?> GetActiveAsync(int tenantid)
		{
			return await _fiscalRepository.GetActiveAsync(tenantid);
		}

		// Herdamos Create, Alter, GetAll, GetByIdAsync, DeleteAsync do BaseService/IGenericRepository.
		// Se quiser lógica adicional (validações) adicione métodos aqui.
		public async Task<FiscalConfiguration> UpdateEntityManually(Model.Registrations.FiscalConfiguration existing, FiscalConfigurationRequest request)
		{
			// 1. NumeracaoDocumentos
			if (request.NumeracaoDocumentos != null)
			{
				if (existing.NumeracaoDocumentos == null)
					existing.NumeracaoDocumentos = new NumeracaoDocumentos();

				// NFe
				if (request.NumeracaoDocumentos.Nfe != null)
				{
					if (existing.NumeracaoDocumentos.Nfe == null)
						existing.NumeracaoDocumentos.Nfe = new NumeracaoItem();

					if (!string.IsNullOrEmpty(request.NumeracaoDocumentos.Nfe.Serie))
						existing.NumeracaoDocumentos.Nfe.Serie = request.NumeracaoDocumentos.Nfe.Serie;

					if (request.NumeracaoDocumentos.Nfe.NumeroInicial > 0)
						existing.NumeracaoDocumentos.Nfe.NumeroInicial = request.NumeracaoDocumentos.Nfe.NumeroInicial;
				}

				// NFCe
				if (request.NumeracaoDocumentos.Nfce != null)
				{
					if (existing.NumeracaoDocumentos.Nfce == null)
						existing.NumeracaoDocumentos.Nfce = new NumeracaoItem();

					if (!string.IsNullOrEmpty(request.NumeracaoDocumentos.Nfce.Serie))
						existing.NumeracaoDocumentos.Nfce.Serie = request.NumeracaoDocumentos.Nfce.Serie;

					if (request.NumeracaoDocumentos.Nfce.NumeroInicial > 0)
						existing.NumeracaoDocumentos.Nfce.NumeroInicial = request.NumeracaoDocumentos.Nfce.NumeroInicial;
				}
			}

			// 2. CertificadoDigital
			if (request.CertificadoDigital != null)
			{
				if (existing.CertificadoDigital == null)
					existing.CertificadoDigital = new CertificadoDigital();

				if (!string.IsNullOrEmpty(request.CertificadoDigital.Arquivo))
					existing.CertificadoDigital.Arquivo = request.CertificadoDigital.Arquivo;

				if (!string.IsNullOrEmpty(request.CertificadoDigital.Senha))
					existing.CertificadoDigital.Senha = request.CertificadoDigital.Senha;
			}

			// 3. Csc
			if (request.Csc != null)
			{
				if (existing.Csc == null)
					existing.Csc = new Csc();

				if (!string.IsNullOrEmpty(request.Csc.Identificador))
					existing.Csc.Identificador = request.Csc.Identificador;

				if (!string.IsNullOrEmpty(request.Csc.Valor))
					existing.Csc.Valor = request.Csc.Valor;
			}

			// 4. Ambiente
			// Verifica se o valor foi enviado (assumindo que 0 é um valor padrão não válido)
			// Ajuste conforme seu enum
			if (request.Ambiente != 0)
				existing.Ambiente = request.Ambiente;

			// 5. Emitente
			if (request.Emitente != null)
			{
				if (existing.Emitente == null)
					existing.Emitente = new Emitente();

				// Propriedades básicas do Emitente
				if (!string.IsNullOrEmpty(request.Emitente.Cnpj))
					existing.Emitente.Cnpj = request.Emitente.Cnpj;

				if (!string.IsNullOrEmpty(request.Emitente.Cpf))
					existing.Emitente.Cpf = request.Emitente.Cpf;

				if (!string.IsNullOrEmpty(request.Emitente.InscricaoEstadual))
					existing.Emitente.InscricaoEstadual = request.Emitente.InscricaoEstadual;

				if (!string.IsNullOrEmpty(request.Emitente.RazaoSocial))
					existing.Emitente.RazaoSocial = request.Emitente.RazaoSocial;

				if (!string.IsNullOrEmpty(request.Emitente.Fantasia))
					existing.Emitente.Fantasia = request.Emitente.Fantasia;

				// Contato
				if (request.Emitente.EmitenteContato != null)
				{
					if (existing.Emitente.EmitenteContato == null)
						existing.Emitente.EmitenteContato = new Contato();

					if (!string.IsNullOrEmpty(request.Emitente.EmitenteContato.Telefone))
						existing.Emitente.EmitenteContato.Telefone = request.Emitente.EmitenteContato.Telefone;
				}

				// Endereco
				if (request.Emitente.EmitenteEndereco != null)
				{
					if (existing.Emitente.EmitenteEndereco == null)
						existing.Emitente.EmitenteEndereco = new Endereco();

					if (!string.IsNullOrEmpty(request.Emitente.EmitenteEndereco.Cep))
						existing.Emitente.EmitenteEndereco.Cep = request.Emitente.EmitenteEndereco.Cep;

					if (!string.IsNullOrEmpty(request.Emitente.EmitenteEndereco.Logradouro))
						existing.Emitente.EmitenteEndereco.Logradouro = request.Emitente.EmitenteEndereco.Logradouro;

					if (!string.IsNullOrEmpty(request.Emitente.EmitenteEndereco.Numero))
						existing.Emitente.EmitenteEndereco.Numero = request.Emitente.EmitenteEndereco.Numero;

					if (!string.IsNullOrEmpty(request.Emitente.EmitenteEndereco.Complemento))
						existing.Emitente.EmitenteEndereco.Complemento = request.Emitente.EmitenteEndereco.Complemento;

					if (!string.IsNullOrEmpty(request.Emitente.EmitenteEndereco.Bairro))
						existing.Emitente.EmitenteEndereco.Bairro = request.Emitente.EmitenteEndereco.Bairro;

					if (!string.IsNullOrEmpty(request.Emitente.EmitenteEndereco.CodigoCidade))
						existing.Emitente.EmitenteEndereco.CodigoCidade = request.Emitente.EmitenteEndereco.CodigoCidade;

					if (!string.IsNullOrEmpty(request.Emitente.EmitenteEndereco.Cidade))
						existing.Emitente.EmitenteEndereco.Cidade = request.Emitente.EmitenteEndereco.Cidade;

					if (!string.IsNullOrEmpty(request.Emitente.EmitenteEndereco.Uf))
						existing.Emitente.EmitenteEndereco.Uf = request.Emitente.EmitenteEndereco.Uf;
				}

				// RegimeTributario
				if (request.Emitente.RegimeTributario != null)
				{
					if (existing.Emitente.RegimeTributario == null)
						existing.Emitente.RegimeTributario = new RegimeTributario();

					if (!string.IsNullOrEmpty(request.Emitente.RegimeTributario.Crt))
						existing.Emitente.RegimeTributario.Crt = request.Emitente.RegimeTributario.Crt;
				}
			}

			// 6. AutorizacaoASO - Sempre atualiza (é um bool)
			existing.AutorizacaoASO = request.AutorizacaoASO;
			return existing;
		}
		public async Task<FiscalConfiguration> CreateEntityFromRequest(FiscalConfigurationRequest request)
		{
			var model = new Model.Registrations.FiscalConfiguration
			{
				// NumeracaoDocumentos
				NumeracaoDocumentos = new NumeracaoDocumentos()
			};

			// 1. NumeracaoDocumentos
			if (request.NumeracaoDocumentos != null)
			{
				// NFe
				if (request.NumeracaoDocumentos.Nfe != null)
				{
					model.NumeracaoDocumentos.Nfe = new NumeracaoItem
					{
						Serie = request.NumeracaoDocumentos.Nfe.Serie ?? string.Empty,
						NumeroInicial = request.NumeracaoDocumentos.Nfe.NumeroInicial
					};
				}

				// NFCe
				if (request.NumeracaoDocumentos.Nfce != null)
				{
					model.NumeracaoDocumentos.Nfce = new NumeracaoItem
					{
						Serie = request.NumeracaoDocumentos.Nfce.Serie ?? string.Empty,
						NumeroInicial = request.NumeracaoDocumentos.Nfce.NumeroInicial
					};
				}
			}

			// 2. CertificadoDigital
			if (request.CertificadoDigital != null)
			{
				model.CertificadoDigital = new CertificadoDigital
				{
					Arquivo = request.CertificadoDigital.Arquivo,
					Senha = request.CertificadoDigital.Senha
				};
			}

			// 3. Csc
			if (request.Csc != null)
			{
				model.Csc = new Csc
				{
					Identificador = request.Csc.Identificador,
					Valor = request.Csc.Valor
				};
			}

			// 4. Ambiente
			model.Ambiente = request.Ambiente;

			// 5. Emitente
			if (request.Emitente != null)
			{
				model.Emitente = new Emitente
				{
					Cnpj = request.Emitente.Cnpj,
					Cpf = request.Emitente.Cpf,
					InscricaoEstadual = request.Emitente.InscricaoEstadual,
					RazaoSocial = request.Emitente.RazaoSocial,
					Fantasia = request.Emitente.Fantasia
				};

				// Contato
				if (request.Emitente.EmitenteContato != null)
				{
					model.Emitente.EmitenteContato = new Contato
					{
						Telefone = request.Emitente.EmitenteContato.Telefone
					};
				}

				// Endereco
				if (request.Emitente.EmitenteEndereco != null)
				{
					model.Emitente.EmitenteEndereco = new Endereco
					{
						Cep = request.Emitente.EmitenteEndereco.Cep,
						Logradouro = request.Emitente.EmitenteEndereco.Logradouro,
						Numero = request.Emitente.EmitenteEndereco.Numero,
						Complemento = request.Emitente.EmitenteEndereco.Complemento,
						Bairro = request.Emitente.EmitenteEndereco.Bairro,
						CodigoCidade = request.Emitente.EmitenteEndereco.CodigoCidade,
						Cidade = request.Emitente.EmitenteEndereco.Cidade,
						Uf = request.Emitente.EmitenteEndereco.Uf
					};
				}

				// RegimeTributario
				if (request.Emitente.RegimeTributario != null)
				{
					model.Emitente.RegimeTributario = new RegimeTributario
					{
						Crt = request.Emitente.RegimeTributario.Crt
					};
				}
			}

			// 6. AutorizacaoASO
			model.AutorizacaoASO = request.AutorizacaoASO;

			return model;
		}
	}
	public interface IFiscalConfigurationService : IBaseService<FiscalConfiguration>
	{
		Task<FiscalConfiguration?> GetActiveAsync(int tenantid);
		Task<FiscalConfiguration> UpdateEntityManually(Model.Registrations.FiscalConfiguration existing, FiscalConfigurationRequest request);
		Task<FiscalConfiguration> CreateEntityFromRequest(FiscalConfigurationRequest request);

	}
}