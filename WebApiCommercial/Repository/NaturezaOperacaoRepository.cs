using Microsoft.EntityFrameworkCore;
using Model.Enums;
using Model.Registrations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
	public class NaturezaOperacaoRepository : GenericRepository<NaturezaOperacao>, INaturezaOperacaoRepository
	{
		public NaturezaOperacaoRepository(ContextBase dbContext) : base(dbContext)
		{

		}

		public async Task<List<NaturezaOperacao>> GetAllAsync(int tenantid)
		{
			return await _dbContext.Set<NaturezaOperacao>()
				.Where(x => x.CompanyId == tenantid)
				.Include(x=>x.RegrasFiscais)
				.AsNoTracking()
				.ToListAsync();
		}

		/// <summary>
		/// Busca natureza de operacao com todas as regras fiscais (matriz) incluidas.
		/// </summary>
		public async Task<NaturezaOperacao?> GetByIdWithRegrasAsync(int naturezaId)
		{
			return await _dbContext.Set<NaturezaOperacao>()
				.Include(n => n.RegrasFiscais)
				.AsNoTracking()
				.FirstOrDefaultAsync(x => x.Id == naturezaId);
		}

		public async Task<bool> ExistsCfopAsync(string cfop, TipoDocumentoEnum tipoDocumento, int id, int idComapny)
		{
			if (id == 0)
			{
				return await _dbContext.Set<NaturezaOperacao>()
					.AsNoTracking()
					.Where(x => x.Cfop == cfop
						&& x.CompanyId == idComapny
						&& x.TipoDocumento == tipoDocumento)
					.AnyAsync();
			}
			else
			{
				return await _dbContext.Set<NaturezaOperacao>()
					.AsNoTracking()
					.Where(x => x.Cfop == cfop
						&& x.TipoDocumento == tipoDocumento
						&& x.CompanyId == idComapny
						&& x.Id != id).AnyAsync();
			}
		}

		public async Task UpdateAsync(NaturezaOperacao entity)
		{
			_dbContext.Set<NaturezaOperacao>().Update(entity);
			await _dbContext.SaveChangesAsync();
		}

		public async Task<NaturezaOperacao?> GetById(int naturezaId)
		{
			return await _dbContext.Set<NaturezaOperacao>()
				.AsNoTracking()
				.FirstOrDefaultAsync(x => x.Id == naturezaId);
		}

		/// <summary>
		/// Busca natureza de operacao com RegrasFiscais incluidas e tracking ativo
		/// para permitir operacoes de atualizacao (upsert/delete/insert) nos filhos.
		/// </summary>
		public async Task<NaturezaOperacao?> GetByIdWithRegrasTrackedAsync(int naturezaId)
		{
			return await _dbContext.Set<NaturezaOperacao>()
				.Include(n => n.RegrasFiscais)
				.AsNoTracking()
				.FirstOrDefaultAsync(x => x.Id == naturezaId);
			
		}
	}

	public interface INaturezaOperacaoRepository : IGenericRepository<NaturezaOperacao>
	{
		Task<List<NaturezaOperacao>> GetAllAsync(int tenantid);
		Task<bool> ExistsCfopAsync(string cfop, TipoDocumentoEnum tipoDocumento, int id, int idComapny);
		Task<NaturezaOperacao?> GetById(int naturezaId);
		Task<NaturezaOperacao?> GetByIdWithRegrasAsync(int naturezaId);
		Task<NaturezaOperacao?> GetByIdWithRegrasTrackedAsync(int naturezaId);
	}
}
