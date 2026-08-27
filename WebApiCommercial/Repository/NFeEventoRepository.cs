using Microsoft.EntityFrameworkCore;
using Model.Registrations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
	public class NFeEventoRepository : GenericRepository<NFeEvento>, INFeEventoRepository
	{
		public NFeEventoRepository(ContextBase dbContext) : base(dbContext)
		{
		}

		/// <summary>
		/// Retorna todos os eventos de uma NF-e, ordenados do mais recente para o mais antigo.
		/// </summary>
		public async Task<List<NFeEvento>> GetByNFeEmissionIdAsync(int nfeEmissionId)
		{
			return await _dbContext.Set<NFeEvento>()
				.Where(x => x.NFeEmissionId == nfeEmissionId)
				.OrderByDescending(x => x.CreatedAt)
				.AsNoTracking()
				.ToListAsync();
		}

		/// <summary>
		/// Conta quantas Cartas de Correção (tpEvento 110110) foram AUTORIZADAS para a NF-e.
		/// Usado para validar o limite de 20 CC-e e calcular o próximo nSeqEvento.
		/// </summary>
		public async Task<int> CountCartaCorrecaoAsync(int nfeEmissionId)
		{
			return await _dbContext.Set<NFeEvento>()
				.CountAsync(x => x.NFeEmissionId == nfeEmissionId
							  && x.TipoEvento == 110110
							  && x.Situacao == SituacaoEvento.Autorizado);
		}
	}

	public interface INFeEventoRepository : IGenericRepository<NFeEvento>
	{
		Task<List<NFeEvento>> GetByNFeEmissionIdAsync(int nfeEmissionId);
		Task<int> CountCartaCorrecaoAsync(int nfeEmissionId);
	}
}
