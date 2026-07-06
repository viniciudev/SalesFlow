using Microsoft.EntityFrameworkCore;
using Model.Enums;
using Model.Registrations;
using Repository;
using Service.Dtos;
using Service.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
	public class NaturezaOperacaoService : BaseService<NaturezaOperacao>, INaturezaOperacaoService
	{
		private readonly IRegrasFiscalRepository _regrasFiscalRepository;
		public NaturezaOperacaoService(IGenericRepository<NaturezaOperacao> repository, IRegrasFiscalRepository regrasFiscalRepository) : base(repository)
		{
			_regrasFiscalRepository = regrasFiscalRepository;
		}

		public async Task<int> CreateAsync(NaturezaOperacaoCreateRequest request)
		{
			await ValidateBusinessRulesAsync(request.Cfop, request.TipoDocumento, request.CompanyId);
			var entity = new NaturezaOperacao
			{
				CompanyId = request.CompanyId,
				Descricao = request.Descricao,
				Cfop = request.Cfop,
				TipoDocumento = request.TipoDocumento,
				Finalidade = request.Finalidade,
				ConsumidorFinal = request.ConsumidorFinal,
				MovimentaEstoque = request.MovimentaEstoque,
				Ativo = request.Ativo,
				PermiteTributacaoPorProduto = request.PermiteTributacaoPorProduto,
				ConfiguracaoTributaria = request.ConfiguracaoTributaria,
				RegrasFiscais = MapRegrasFiscais(request.RegrasFiscais)
			};
			ValidateRegrasFiscaisUnicidade(entity.RegrasFiscais);
			ApplyTipoDocumentoRules(entity); NormalizeTributos(entity);
			await base.Create(entity); return entity.Id;
		}

		public async Task DeleteAsync(int id)
		{
			var existing = await base.GetByIdAsync(id);
			if (existing == null) throw new DomainException("Natureza de operacao nao encontrada.");
			await base.DeleteAsync(id);
		}

		public async Task<List<NaturezaOperacaoResponse>> GetAllAsync(int tenantid)
		{
			var list = await (repository as INaturezaOperacaoRepository).GetAllAsync(tenantid);
			return list.Select(MapToResponse).ToList();
		}

		public async Task<NaturezaOperacaoResponse?> GetByIdAsync(int id)
		{
			var e = await base.GetByIdAsync(id);
			return e == null ? null : MapToResponse(e);
		}

		public async Task UpdateAsync(int id, NaturezaOperacaoUpdateRequest request)
		{
			var hasRegrasFiscais = request.RegrasFiscais != null && request.RegrasFiscais.Any();

			// Se ha RegrasFiscais para atualizar, busca com tracking para gerenciar corretamente os filhos
			NaturezaOperacao existing;
			if (hasRegrasFiscais)
			{
				existing = await (repository as INaturezaOperacaoRepository)
						.GetByIdWithRegrasTrackedAsync(id);
			}
			else
			{
				existing = await base.GetByIdAsync(id);
			}

			if (existing == null) throw new DomainException("Natureza de operacao nao encontrada.");
			if (await (repository as INaturezaOperacaoRepository).ExistsCfopAsync(request.Cfop, request.TipoDocumento, id, existing.CompanyId))
				throw new DomainException("Ja existe uma natureza de operacao com o mesmo CFOP e Tipo de Documento.");
			existing.Descricao = request.Descricao; existing.Cfop = request.Cfop;
			existing.TipoDocumento = request.TipoDocumento; existing.Finalidade = request.Finalidade;
			existing.ConsumidorFinal = request.ConsumidorFinal; existing.MovimentaEstoque = request.MovimentaEstoque;
			existing.Ativo = request.Ativo; existing.PermiteTributacaoPorProduto = request.PermiteTributacaoPorProduto;
			existing.ConfiguracaoTributaria = request.ConfiguracaoTributaria;

			if (hasRegrasFiscais)
			{
				var novasRegras = MapRegrasFiscais(request.RegrasFiscais);
				ValidateRegrasFiscaisUnicidade(novasRegras);
				existing.RegrasFiscais.Clear();
				foreach (var regra in novasRegras)
				{
					regra.NaturezaOperacaoId = id;
					regra.Id = 0;
					existing.RegrasFiscais.Add(regra);
				}
			}

			ApplyTipoDocumentoRules(existing); NormalizeTributos(existing);

			if (hasRegrasFiscais)
			{
				await (repository as INaturezaOperacaoRepository).GetDbContext().SaveChangesAsync();
			}
			else
			{
				await base.Alter(existing);
			}
		}

		// ========== MELHORIA 2: Batch endpoint para RegrasFiscais ==========
		public async Task AtualizarRegrasFiscaisBatchAsync(int naturezaOperacaoId, List<RegraFiscalDto> regras)
		{
			var natureza = await (repository as INaturezaOperacaoRepository)
					.GetByIdWithRegrasTrackedAsync(naturezaOperacaoId);
			if (natureza == null) throw new DomainException("Natureza de operacao nao encontrada.");

			var novasRegras = MapRegrasFiscais(regras);
			ValidateRegrasFiscaisUnicidade(novasRegras);

			// Remove todas as regras existentes e insere as novas.
			// EF Core: ao limpar a colecao e adicionar novos itens, o ChangeTracker
			// gera DELETE para os registros antigos e INSERT para os novos,
			// evitando violacao da unique constraint (IX_tb_regraFiscal_NaturezaOperacaoId_SituacaoTributariaId_Destino).
	
			foreach (var item in natureza.RegrasFiscais)
			{
				await _regrasFiscalRepository.DeleteAsync(item.Id);
			}
			foreach (var regra in novasRegras)
			{
				regra.NaturezaOperacaoId = naturezaOperacaoId;
				regra.Id = 0; // Forca INSERT para todos, evitando conflito de IDs
				await _regrasFiscalRepository.CreateAsync(regra);

			}
		}

		// ========== MELHORIA 4: Clonar matriz de uma natureza para outra ==========
		public async Task ClonarMatrizAsync(int origemId, int destinoId)
		{
			var origem = await (repository as INaturezaOperacaoRepository).GetByIdWithRegrasAsync(origemId);
			if (origem == null) throw new DomainException("Natureza de operacao origem nao encontrada.");
			var destino = await (repository as INaturezaOperacaoRepository)
					.GetByIdWithRegrasTrackedAsync(destinoId);
			if (destino == null) throw new DomainException("Natureza de operacao destino nao encontrada.");
			if (origem.RegrasFiscais == null || !origem.RegrasFiscais.Any())
				throw new DomainException("Natureza de operacao origem nao possui regras fiscais para clonar.");

			var regrasClonadas = origem.RegrasFiscais.Select(r => new RegraFiscal
			{
				Id = 0,
				NaturezaOperacaoId = destinoId,
				SituacaoTributariaId = r.SituacaoTributariaId,
				Destino = r.Destino,
				Cfop = r.Cfop,
				ConfiguracaoTributaria = CloneConfiguracao(r.ConfiguracaoTributaria)
			}).ToList();

			destino.RegrasFiscais.Clear();
			foreach (var regra in regrasClonadas)
				destino.RegrasFiscais.Add(regra);

			await (repository as INaturezaOperacaoRepository).GetDbContext().SaveChangesAsync();
		}

		// ========== MELHORIA 4: Exportar matriz como CSV ==========
		public async Task<string> ExportarMatrizCsvAsync(int naturezaOperacaoId)
		{
			var natureza = await (repository as INaturezaOperacaoRepository).GetByIdWithRegrasAsync(naturezaOperacaoId);
			if (natureza == null) throw new DomainException("Natureza de operacao nao encontrada.");
			var sb = new StringBuilder();
			sb.AppendLine("SituacaoTributariaId;SituacaoTributaria;Destino;CFOP;CST_ICMS;CSOSN;Aliq_ICMS;Reduz_Base;CST_PIS;Aliq_PIS;CST_COFINS;Aliq_COFINS;CST_IBS;Aliq_IBS;CST_CBS;Aliq_CBS;Aliq_IS;cClassTrib");
			foreach (var rf in natureza.RegrasFiscais ?? new List<RegraFiscal>())
			{
				var cfg = rf.ConfiguracaoTributaria ?? new ConfiguracaoTributaria();
				sb.AppendLine(string.Join(";",
						rf.SituacaoTributariaId, rf.SituacaoTributaria?.Codigo ?? "", rf.Destino.ToString(), rf.Cfop,
						cfg.CstICMS ?? "", cfg.CsosnICMS ?? "", cfg.AliquotaICMS, cfg.ReduzirBaseICMS,
						cfg.CstPIS ?? "", cfg.AliquotaPIS, cfg.CstCOFINS ?? "", cfg.AliquotaCOFINS,
						cfg.CstIBS ?? "", cfg.AliquotaIBS, cfg.CstCBS ?? "", cfg.AliquotaCBS,
						cfg.AliquotaIS, cfg.cClassTrib ?? ""));
			}
			return sb.ToString();
		}

		// ========== MELHORIA 6: Migrar produtos do modelo antigo para o novo ==========
		public async Task<object> MigrarProdutosParaMatrizAsync(int tenantId)
		{
			var db = (repository as INaturezaOperacaoRepository).GetDbContext();
			var todosProdutos = await db.Set<Product>()
					.Where(p => p.IdCompany == tenantId && p.UsaTributacaoPropria)
					.ToListAsync();

			int migrados = 0, ignorados = 0;
			var erros = new List<string>();

			foreach (var produto in todosProdutos)
			{
				try
				{
					if (produto.SituacaoTributariaId.HasValue && produto.SituacaoTributariaId > 0)
					{ ignorados++; continue; }

					var situacaoMigrada = await db.Set<SituacaoTributaria>()
							.FirstOrDefaultAsync(s => s.CompanyId == tenantId && s.Codigo == "MIGRADA");
					if (situacaoMigrada == null)
					{
						situacaoMigrada = new SituacaoTributaria
						{ CompanyId = tenantId, Codigo = "MIGRADA", Descricao = "Produto migrado do modelo antigo" };
						db.Set<SituacaoTributaria>().Add(situacaoMigrada);
						await db.SaveChangesAsync();
					}
					produto.SituacaoTributariaId = situacaoMigrada.Id;
					produto.UsaTributacaoPropria = false;
					produto.DataAtualizacaoTributaria = DateTime.UtcNow;
					db.Set<Product>().Update(produto);
					await db.SaveChangesAsync();
					migrados++;
				}
				catch (Exception ex) { erros.Add("Produto " + produto.Id + " (" + produto.Name + "): " + ex.Message); }
			}
			return new { TotalProdutos = todosProdutos.Count, Migrados = migrados, Ignorados = ignorados, Erros = erros };
		}

		// ========== METODOS AUXILIARES ==========

		private void ValidateRegrasFiscaisUnicidade(ICollection<RegraFiscal> regras)
		{
			if (regras == null || !regras.Any()) return;
			var duplicadas = regras
					.GroupBy(r => new { r.SituacaoTributariaId, r.Destino })
					.Where(g => g.Count() > 1).ToList();
			if (duplicadas.Any())
			{
				var desc = string.Join("; ", duplicadas.Select(g =>
						"SituacaoTributariaId=" + g.Key.SituacaoTributariaId + " Destino=" + g.Key.Destino));
				throw new DomainException("Regras fiscais duplicadas na matriz: " + desc);
			}
			var cfopsDuplicados = regras.GroupBy(r => r.Cfop).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
			if (cfopsDuplicados.Any())
				throw new DomainException("CFOPs duplicados na matriz: " + string.Join(", ", cfopsDuplicados) + ". Cada combinacao SituacaoTributaria+Destino deve ter CFOP unico.");
		}

		private ConfiguracaoTributaria CloneConfiguracao(ConfiguracaoTributaria origem)
		{
			if (origem == null) return new ConfiguracaoTributaria();
			return new ConfiguracaoTributaria
			{
				AplicarICMS = origem.AplicarICMS,
				CstICMS = origem.CstICMS,
				CsosnICMS = origem.CsosnICMS,
				AliquotaICMS = origem.AliquotaICMS,
				ReduzirBaseICMS = origem.ReduzirBaseICMS,
				AplicarIPI = origem.AplicarIPI,
				CstIPI = origem.CstIPI,
				AliquotaIPI = origem.AliquotaIPI,
				AplicarPIS = origem.AplicarPIS,
				CstPIS = origem.CstPIS,
				AliquotaPIS = origem.AliquotaPIS,
				AplicarCOFINS = origem.AplicarCOFINS,
				CstCOFINS = origem.CstCOFINS,
				AliquotaCOFINS = origem.AliquotaCOFINS,
				AplicarISSQN = origem.AplicarISSQN,
				AliquotaISSQN = origem.AliquotaISSQN,
				AplicarIBS = origem.AplicarIBS,
				CstIBS = origem.CstIBS,
				AliquotaIBS = origem.AliquotaIBS,
				AplicarCBS = origem.AplicarCBS,
				CstCBS = origem.CstCBS,
				AliquotaCBS = origem.AliquotaCBS,
				AplicarIS = origem.AplicarIS,
				AliquotaIS = origem.AliquotaIS,
				cClassTrib = origem.cClassTrib,
			};
		}

		public async Task<object> GerarPayloadFiscalAsync(int id)
		{
			var e = await base.GetByIdAsync(id);
			if (e == null) throw new DomainException("Natureza de operacao nao encontrada.");
			var tributos = new List<object>();
			void AddTributo(string nome, bool aplicar, string? cst, decimal aliquota)
			{ tributos.Add(new { Nome = nome, Cst = cst ?? string.Empty, Aliquota = aplicar ? aliquota : 0m }); }
			var cfg = e.ConfiguracaoTributaria;
			AddTributo("ICMS", cfg.AplicarICMS, !string.IsNullOrEmpty(cfg.CsosnICMS) ? cfg.CsosnICMS : cfg.CstICMS, cfg.AliquotaICMS);
			AddTributo("IPI", cfg.AplicarIPI, cfg.CstIPI, cfg.AliquotaIPI);
			AddTributo("PIS", cfg.AplicarPIS, cfg.CstPIS, cfg.AliquotaPIS);
			AddTributo("COFINS", cfg.AplicarCOFINS, cfg.CstCOFINS, cfg.AliquotaCOFINS);
			AddTributo("ISSQN", cfg.AplicarISSQN, null, cfg.AliquotaISSQN);
			AddTributo("IBS", cfg.AplicarIBS, cfg.CstIBS, cfg.AliquotaIBS);
			AddTributo("CBS", cfg.AplicarCBS, cfg.CstCBS, cfg.AliquotaCBS);
			AddTributo("IS", cfg.AplicarIS, null, cfg.AliquotaIS);
			return new { Cfop = e.Cfop, Finalidade = e.Finalidade.ToString(), ConsumidorFinal = e.ConsumidorFinal, Tributos = tributos };
		}

		private NaturezaOperacaoResponse MapToResponse(NaturezaOperacao e)
		{
			return new NaturezaOperacaoResponse
			{
				Id = e.Id,
				Descricao = e.Descricao,
				Cfop = e.Cfop,
				TipoDocumento = e.TipoDocumento,
				Finalidade = e.Finalidade,
				ConsumidorFinal = e.ConsumidorFinal,
				MovimentaEstoque = e.MovimentaEstoque,
				Ativo = e.Ativo,
				PermiteTributacaoPorProduto = e.PermiteTributacaoPorProduto,
				ConfiguracaoTributaria = e.ConfiguracaoTributaria,
				RegrasFiscais = e.RegrasFiscais?.Select(r => new RegraFiscalDto
				{
					Id = r.Id,
					SituacaoTributariaId = r.SituacaoTributariaId,
					SituacaoTributariaCodigo = r.SituacaoTributaria?.Codigo,
					SituacaoTributariaDescricao = r.SituacaoTributaria?.Descricao,
					Destino = r.Destino,
					Cfop = r.Cfop,
					ConfiguracaoTributaria = r.ConfiguracaoTributaria ?? new ConfiguracaoTributaria()
				}).ToList() ?? new List<RegraFiscalDto>()
			};
		}

		private List<RegraFiscal> MapRegrasFiscais(List<RegraFiscalDto> dtos)
		{
			return dtos.Select(dto => new RegraFiscal
			{
				Id = dto.Id ?? 0,
				SituacaoTributariaId = dto.SituacaoTributariaId,
				Destino = dto.Destino,
				Cfop = dto.Cfop,
				ConfiguracaoTributaria = dto.ConfiguracaoTributaria ?? new ConfiguracaoTributaria()
			}).ToList();
		}

		private async Task ValidateBusinessRulesAsync(string cfop, TipoDocumentoEnum tipoDocumento, int companyId)
		{
			if (await (repository as INaturezaOperacaoRepository).ExistsCfopAsync(cfop, tipoDocumento, 0, companyId))
				throw new DomainException("Ja existe uma natureza de operacao com o mesmo CFOP e Tipo de Documento.");
		}

		private void ApplyTipoDocumentoRules(NaturezaOperacao entity)
		{
			if (entity.TipoDocumento == TipoDocumentoEnum.NFCE)
			{
				entity.ConsumidorFinal = true; entity.Finalidade = FinalidadeEnum.NORMAL;
				entity.ConfiguracaoTributaria.AplicarISSQN = false; entity.ConfiguracaoTributaria.AliquotaISSQN = 0m;
			}
		}

		private void NormalizeTributos(NaturezaOperacao entity)
		{
			var c = entity.ConfiguracaoTributaria;
			if (!c.AplicarICMS) { c.AliquotaICMS = 0m; c.CstICMS = null; c.CsosnICMS = null; }
			if (!c.AplicarIPI) { c.AliquotaIPI = 0m; c.CstIPI = null; }
			if (!c.AplicarPIS) { c.AliquotaPIS = 0m; c.CstPIS = null; }
			if (!c.AplicarCOFINS) { c.AliquotaCOFINS = 0m; c.CstCOFINS = null; }
			if (!c.AplicarISSQN) { c.AliquotaISSQN = 0m; }
			if (!c.AplicarIBS) { c.AliquotaIBS = 0m; c.CstIBS = null; }
			if (!c.AplicarCBS) { c.AliquotaCBS = 0m; c.CstCBS = null; }
			if (!c.AplicarIS) { c.AliquotaIS = 0m; }
			if (c.AplicarICMS && c.AliquotaICMS <= 0m) throw new DomainException("Aliquota ICMS deve ser maior que zero quando ICMS estiver aplicado.");
			if (c.AplicarICMS && string.IsNullOrEmpty(c.CstICMS) && string.IsNullOrEmpty(c.CsosnICMS)) throw new DomainException("CST ou CSOSN ICMS deve ser informado quando ICMS estiver aplicado.");
			if (c.AplicarIPI && c.AliquotaIPI <= 0m) throw new DomainException("Aliquota IPI deve ser maior que zero quando IPI estiver aplicado.");
			if (c.AplicarPIS && c.AliquotaPIS <= 0m) throw new DomainException("Aliquota PIS deve ser maior que zero quando PIS estiver aplicado.");
			if (c.AplicarCOFINS && c.AliquotaCOFINS <= 0m) throw new DomainException("Aliquota COFINS deve ser maior que zero quando COFINS estiver aplicado.");
			if (c.AplicarISSQN && c.AliquotaISSQN <= 0m) throw new DomainException("Aliquota ISSQN deve ser maior que zero quando ISSQN estiver aplicado.");
			if (c.AplicarIBS && c.AliquotaIBS <= 0m) throw new DomainException("Aliquota IBS deve ser maior que zero quando IBS estiver aplicado.");
			if (c.AplicarCBS && c.AliquotaCBS <= 0m) throw new DomainException("Aliquota CBS deve ser maior que zero quando CBS estiver aplicado.");
			if (c.AplicarIS && c.AliquotaIS <= 0m) throw new DomainException("Aliquota IS deve ser maior que zero quando IS estiver aplicado.");
		}
	}

	public interface INaturezaOperacaoService : IBaseService<NaturezaOperacao>
	{
		Task<int> CreateAsync(NaturezaOperacaoCreateRequest request);
		Task UpdateAsync(int id, NaturezaOperacaoUpdateRequest request);
		Task DeleteAsync(int id);
		Task<NaturezaOperacaoResponse?> GetByIdAsync(int id);
		Task<List<NaturezaOperacaoResponse>> GetAllAsync(int tenantid);
		Task<object> GerarPayloadFiscalAsync(int id);
		Task AtualizarRegrasFiscaisBatchAsync(int naturezaOperacaoId, List<RegraFiscalDto> regras);
		Task ClonarMatrizAsync(int origemId, int destinoId);
		Task<string> ExportarMatrizCsvAsync(int naturezaOperacaoId);
		Task<object> MigrarProdutosParaMatrizAsync(int tenantId);
	}
}
