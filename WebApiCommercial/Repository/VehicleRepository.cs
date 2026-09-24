using Microsoft.EntityFrameworkCore;
using Model;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    /// <summary>
    /// Persistência do cadastro de veículos (MDF-e).
    ///
    /// Duas observações que valem para todo método aqui:
    ///
    /// 1. <b>Isolamento por empresa é manual.</b> O <c>TenantQueryInterceptor</c>
    ///    está comentado em <c>ContextBase.OnConfiguring</c>, então NÃO existe
    ///    filtro automático de tenant. Todo método que devolve veículo filtra
    ///    <c>IdCompany</c> explicitamente — inclusive os "GetById", porque o
    ///    <c>GenericRepository.GetByIdAsync(int)</c> herdado não filtra empresa e
    ///    vazaria veículo de outra empresa.
    ///
    /// 2. <b>Isolamento de exclusão também é manual.</b> Não há
    ///    <c>HasQueryFilter</c> nem <c>ISoftDeletable</c> no projeto: o
    ///    <c>IsActive</c> (RV11) é filtrado à mão em cada consulta que precisa
    ///    dele. A listagem mostra ativos E inativos de propósito; só a seleção
    ///    para MDF-e (RV12) restringe a ativos.
    /// </summary>
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(ContextBase dbContext) : base(dbContext) { }

        /// <summary>
        /// Listagem paginada do cadastro. Traz ATIVOS E INATIVOS (a tela tem
        /// coluna "Ativo" e o filtro correspondente) — diferente de
        /// <see cref="GetActiveForMdfEAsync"/>, que é a seleção para o manifesto.
        /// </summary>
        public async Task<PagedResult<Vehicle>> GetPagedAsync(Filters filter)
        {
            var query = _dbContext.Set<Vehicle>()
                .Include(v => v.Client)
                .Where(v => v.IdCompany == filter.IdCompany);

            // Busca textual única (TextOption) cobrindo placa E código interno:
            // a placa não tem campo próprio no filtro justamente para que um só
            // parâmetro atenda "acha pelo que o usuário tem em mãos" — a placa
            // ele lê no veículo, o código interno ele lê na frota.
            if (!string.IsNullOrWhiteSpace(filter.TextOption))
            {
                var search = filter.TextOption.Trim();
                query = query.Where(v => v.LicensePlate.Contains(search)
                    || (v.InternalCode != null && v.InternalCode.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(filter.LicensingState))
            {
                var uf = filter.LicensingState.Trim().ToUpper();
                query = query.Where(v => v.LicensingState == uf);
            }

            if (filter.VehicleType.HasValue)
                query = query.Where(v => v.VehicleType == filter.VehicleType.Value);

            // null = todos. Ver o comentário em Filters.VehicleIsActive: o padrão
            // do repositório é NÃO filtrar, e a tela decide se quer só ativos.
            if (filter.VehicleIsActive.HasValue)
                query = query.Where(v => v.IsActive == filter.VehicleIsActive.Value);

            return await query
                .AsNoTracking()
                .WithCaseInsensitive()
                .OrderByDescending(v => v.Id)
                .GetPagedAsync(filter.PageNumber, filter.PageSize);
        }

        /// <summary>
        /// Veículo por Id, restrito à empresa. Filtra <c>IdCompany</c> de
        /// propósito: o <c>GetByIdAsync(int)</c> herdado não filtra, e usá-lo
        /// aqui permitiria ler (e depois alterar) veículo de outra empresa
        /// passando o Id na URL.
        ///
        /// <c>Include(Client)</c> é obrigatório: sem ele, o <c>Vehicle</c>
        /// voltaria com <c>Client</c> nulo, o DTO de resposta perderia os dados
        /// do proprietário (RV09/RV10) e a tela mostraria o parceiro em branco.
        /// </summary>
        public async Task<Vehicle> GetByIdAsync(int id, int idCompany)
        {
            return await _dbContext.Set<Vehicle>()
                .Include(v => v.Client)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id && v.IdCompany == idCompany);
        }

        /// <summary>
        /// Busca por placa dentro da empresa.
        ///
        /// <paramref name="onlyActive"/> existe para desambiguar a mensagem de
        /// RV02: com <c>true</c>, acha só o que realmente conflita; com
        /// <c>false</c>, acha também o desativado — que é permitido e não deve
        /// virar erro, mas cuja existência vale mencionar na tela.
        /// </summary>
        public async Task<Vehicle> GetByLicensePlateAsync(int idCompany, string plate, bool onlyActive = true)
        {
            if (string.IsNullOrWhiteSpace(plate))
                return null;

            var normalized = VehiclePlate.Normalize(plate);

            var query = _dbContext.Set<Vehicle>()
                .AsNoTracking()
                .Where(v => v.IdCompany == idCompany && v.LicensePlate == normalized);

            if (onlyActive)
                query = query.Where(v => v.IsActive);

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// RV02 — já existe veículo ATIVO com esta placa nesta empresa?
        ///
        /// O <c>IsActive</c> aqui é a regra em si, não uma otimização: a placa é
        /// única apenas entre ativos, para que desativar um veículo libere o
        /// recadastro da mesma placa (é o índice único PARCIAL do banco que
        /// garante isso, e esta consulta é a versão em código dele).
        ///
        /// <paramref name="ignoreId"/> existe para a edição: o veículo sendo
        /// alterado não pode ser considerado conflito consigo mesmo.
        /// </summary>
        public async Task<bool> ExistsActivePlateAsync(int idCompany, string plate, int? ignoreId = null)
        {
            if (string.IsNullOrWhiteSpace(plate))
                return false;

            var normalized = VehiclePlate.Normalize(plate);

            return await _dbContext.Set<Vehicle>()
                .AsNoTracking()
                .AnyAsync(v => v.IdCompany == idCompany
                    && v.LicensePlate == normalized
                    && v.IsActive
                    && (ignoreId == null || v.Id != ignoreId.Value));
        }

        /// <summary>
        /// RV12 — veículos disponíveis para compor um MDF-e.
        ///
        /// Filtra <c>IsActive</c> porque o manifesto é documento fiscal: oferecer
        /// um veículo desativado na seleção é oferecer uma rejeição da SEFAZ.
        /// Traz o <c>Client</c> incluído porque o checklist de RV10 (RNTRC e
        /// documento do proprietário) depende dele, e sem o Include o veículo
        /// terceiro apareceria sempre como inapto.
        /// </summary>
        public async Task<List<Vehicle>> GetActiveForMdfEAsync(int idCompany)
        {
            return await _dbContext.Set<Vehicle>()
                .Include(v => v.Client)
                .AsNoTracking()
                .Where(v => v.IdCompany == idCompany && v.IsActive)
                .OrderBy(v => v.LicensePlate)
                .ToListAsync();
        }

        /// <summary>
        /// Grava um veículo novo, carimbando as datas de auditoria.
        ///
        /// Não usa o <c>CreateAsync</c> herdado porque ele só faz
        /// <c>AddAsync</c> + <c>SaveChangesAsync</c>, sem tocar em
        /// <c>CreatedAt/UpdatedAt</c> — e, como não há interceptor de auditoria
        /// no projeto, quem grava precisa preenchê-las.
        /// </summary>
        public async Task<Vehicle> AddAsync(Vehicle vehicle)
        {
            var now = DateTime.UtcNow;
            vehicle.CreatedAt = now;
            vehicle.UpdatedAt = now;

            await _dbContext.Set<Vehicle>().AddAsync(vehicle);
            await _dbContext.SaveChangesAsync();

            return vehicle;
        }

        /// <summary>
        /// Atualiza um veículo existente.
        ///
        /// <c>Update</c> do EF marca a entidade inteira como modificada, então a
        /// entidade recebida precisa vir completa (o serviço monta campo a campo
        /// a partir do DTO sobre o registro já carregado) — mandar um objeto
        /// parcial aqui zeraria as colunas ausentes.
        ///
        /// <c>CreatedAt</c> é relido do banco: sem isso, o <c>Update</c>
        /// sobrescreveria a data de criação com o que veio no objeto, e a
        /// auditoria se perderia a cada edição.
        /// </summary>
        public async Task UpdateAsync(Vehicle vehicle)
        {
            var createdAt = await _dbContext.Set<Vehicle>()
                .AsNoTracking()
                .Where(v => v.Id == vehicle.Id)
                .Select(v => (DateTime?)v.CreatedAt)
                .FirstOrDefaultAsync();

            if (createdAt.HasValue)
                vehicle.CreatedAt = createdAt.Value;

            vehicle.UpdatedAt = DateTime.UtcNow;

            _dbContext.Set<Vehicle>().Update(vehicle);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// RV11 — desativar/reativar é sempre lógico. Marca <c>IsActive</c> e
        /// NADA mais: não existe DELETE físico de veículo no sistema.
        ///
        /// O <c>ExecuteUpdate</c> evita carregar a entidade inteira só para
        /// trocar um booleano, e — o motivo prático — não mexe em navegações: um
        /// <c>Update</c> na entidade carregada regravaria o <c>Client</c> junto,
        /// e o parceiro seria sobrescrito por um objeto possivelmente
        /// desatualizado. Devolve quantas linhas mudaram, para o serviço
        /// distinguir "não existe / é de outra empresa" de "ok".
        /// </summary>
        public async Task<int> SetActiveAsync(int id, int idCompany, bool isActive)
        {
            return await _dbContext.Set<Vehicle>()
                .Where(v => v.Id == id && v.IdCompany == idCompany)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(v => v.IsActive, isActive)
                    .SetProperty(v => v.UpdatedAt, DateTime.UtcNow));
        }
    }

    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        Task<PagedResult<Vehicle>> GetPagedAsync(Filters filter);

        /// <summary>
        /// Versão com escopo de empresa. O <c>GetByIdAsync(int)</c> herdado de
        /// <see cref="IGenericRepository{TEntity}"/> NÃO filtra empresa — não o
        /// use para veículo.
        /// </summary>
        Task<Vehicle> GetByIdAsync(int id, int idCompany);
        Task<Vehicle> GetByLicensePlateAsync(int idCompany, string plate, bool onlyActive = true);
        Task<bool> ExistsActivePlateAsync(int idCompany, string plate, int? ignoreId = null);
        Task<List<Vehicle>> GetActiveForMdfEAsync(int idCompany);
        Task<Vehicle> AddAsync(Vehicle vehicle);
        Task UpdateAsync(Vehicle vehicle);
        Task<int> SetActiveAsync(int id, int idCompany, bool isActive);
    }
}
