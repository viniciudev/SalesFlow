using Model.Enums;
using Model.Registrations;
using Repository;
using Service.Dtos;
using Service.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace Service
{
    public class NaturezaOperacaoService : BaseService<NaturezaOperacao>, INaturezaOperacaoService
    {
        public NaturezaOperacaoService(IGenericRepository<NaturezaOperacao> repository) : base(repository)
        {
        }

        public async Task<int> CreateAsync(NaturezaOperacaoCreateRequest request)
        {
            await ValidateBusinessRulesAsync(request.Cfop, request.TipoDocumento);

            var entity = new NaturezaOperacao
            {
               
                Descricao = request.Descricao,
                Cfop = request.Cfop,
                TipoDocumento = request.TipoDocumento,
                Finalidade = request.Finalidade,
                ConsumidorFinal = request.ConsumidorFinal,
                MovimentaEstoque = request.MovimentaEstoque,
                Ativo = request.Ativo,
                ConfiguracaoTributaria = request.ConfiguracaoTributaria
            };

            ApplyTipoDocumentoRules(entity);
            NormalizeTributos(entity);

            await base.Create(entity);
            return entity.Id;
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await base.GetByIdAsync(id);
            if (existing == null)
                throw new DomainException("Natureza de operação não encontrada.");

            await base.DeleteAsync(id);
        }

        public async Task<List<NaturezaOperacaoResponse>> GetAllAsync()
        {
            var list = await ( repository as INaturezaOperacaoRepository).GetAllAsync();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<NaturezaOperacaoResponse?> GetByIdAsync(int id)
        {
            var e = await base.GetByIdAsync(id);
            return e == null ? null : MapToResponse(e);
        }

        public async Task UpdateAsync(int id, NaturezaOperacaoUpdateRequest request)
        {
            var existing = await base.GetByIdAsync(id);
            if (existing == null)
                throw new DomainException("Natureza de operação não encontrada.");

            if (await (repository as INaturezaOperacaoRepository).ExistsCfopAsync(request.Cfop, request.TipoDocumento))
                throw new DomainException("Já existe uma natureza de operação com o mesmo CFOP e Tipo de Documento.");

            existing.Descricao = request.Descricao;
            existing.Cfop = request.Cfop;
            existing.TipoDocumento = request.TipoDocumento;
            existing.Finalidade = request.Finalidade;
            existing.ConsumidorFinal = request.ConsumidorFinal;
            existing.MovimentaEstoque = request.MovimentaEstoque;
            existing.Ativo = request.Ativo;
            existing.ConfiguracaoTributaria = request.ConfiguracaoTributaria;

            ApplyTipoDocumentoRules(existing);
            NormalizeTributos(existing);

            await base.Alter(existing);
        }

        public async Task<object> GerarPayloadFiscalAsync(int id)
        {
            var e = await base.GetByIdAsync(id);
            if (e == null)
                throw new DomainException("Natureza de operação não encontrada.");

            var tributos = new List<object>();

            void AddTributo(string nome, bool aplicar, string? cst, decimal aliquota)
            {
                tributos.Add(new { Nome = nome, Cst = cst ?? string.Empty, Aliquota = aplicar ? aliquota : 0m });
            }

            var cfg = e.ConfiguracaoTributaria;

            AddTributo("ICMS", cfg.AplicarICMS, cfg.CstICMS, cfg.AliquotaICMS);
            AddTributo("IPI", cfg.AplicarIPI, cfg.CstIPI, cfg.AliquotaIPI);
            AddTributo("PIS", cfg.AplicarPIS, cfg.CstPIS, cfg.AliquotaPIS);
            AddTributo("COFINS", cfg.AplicarCOFINS, cfg.CstCOFINS, cfg.AliquotaCOFINS);
            AddTributo("ISSQN", cfg.AplicarISSQN, null, cfg.AliquotaISSQN);
            AddTributo("IBS", cfg.AplicarIBS, cfg.CstIBS, cfg.AliquotaIBS);
            AddTributo("CBS", cfg.AplicarCBS, cfg.CstCBS, cfg.AliquotaCBS);
            AddTributo("IS", cfg.AplicarIS, null, cfg.AliquotaIS);

            return new
            {
                Cfop = e.Cfop,
                Finalidade = e.Finalidade.ToString(),
                ConsumidorFinal = e.ConsumidorFinal,
                Tributos = tributos
            };
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
                ConfiguracaoTributaria = e.ConfiguracaoTributaria
            };
        }

        private async Task ValidateBusinessRulesAsync(string cfop, TipoDocumentoEnum tipoDocumento)
        {
            // Unicidade CFOP + TipoDocumento
            if (await (repository as INaturezaOperacaoRepository).ExistsCfopAsync(cfop, tipoDocumento))
                throw new DomainException("Já existe uma natureza de operação com o mesmo CFOP e Tipo de Documento.");
        }

        private void ApplyTipoDocumentoRules(NaturezaOperacao entity)
        {
            if (entity.TipoDocumento == TipoDocumentoEnum.NFCE)
            {
                entity.ConsumidorFinal = true;
                entity.Finalidade = FinalidadeEnum.NORMAL;
                entity.ConfiguracaoTributaria.AplicarISSQN = false;
                entity.ConfiguracaoTributaria.AliquotaISSQN = 0m;
            }
        }

        private void NormalizeTributos(NaturezaOperacao entity)
        {
            var c = entity.ConfiguracaoTributaria;

            // If aplicar == false => zero aliquota
            if (!c.AplicarICMS) c.AliquotaICMS = 0m;
            if (!c.AplicarIPI) c.AliquotaIPI = 0m;
            if (!c.AplicarPIS) c.AliquotaPIS = 0m;
            if (!c.AplicarCOFINS) c.AliquotaCOFINS = 0m;
            if (!c.AplicarISSQN) c.AliquotaISSQN = 0m;
            if (!c.AplicarIBS) c.AliquotaIBS = 0m;
            if (!c.AplicarCBS) c.AliquotaCBS = 0m;
            if (!c.AplicarIS) c.AliquotaIS = 0m;

            // Validate aliquotas > 0 when aplicar == true
            if (c.AplicarICMS && c.AliquotaICMS <= 0m)
                throw new DomainException("Aliquota ICMS deve ser maior que zero quando ICMS estiver aplicado.");
            if (c.AplicarIPI && c.AliquotaIPI <= 0m)
                throw new DomainException("Aliquota IPI deve ser maior que zero quando IPI estiver aplicado.");
            if (c.AplicarPIS && c.AliquotaPIS <= 0m)
                throw new DomainException("Aliquota PIS deve ser maior que zero quando PIS estiver aplicado.");
            if (c.AplicarCOFINS && c.AliquotaCOFINS <= 0m)
                throw new DomainException("Aliquota COFINS deve ser maior que zero quando COFINS estiver aplicado.");
            if (c.AplicarISSQN && c.AliquotaISSQN <= 0m)
                throw new DomainException("Aliquota ISSQN deve ser maior que zero quando ISSQN estiver aplicado.");
            if (c.AplicarIBS && c.AliquotaIBS <= 0m)
                throw new DomainException("Aliquota IBS deve ser maior que zero quando IBS estiver aplicado.");
            if (c.AplicarCBS && c.AliquotaCBS <= 0m)
                throw new DomainException("Aliquota CBS deve ser maior que zero quando CBS estiver aplicado.");
            if (c.AplicarIS && c.AliquotaIS <= 0m)
                throw new DomainException("Aliquota IS deve ser maior que zero quando IS estiver aplicado.");
        }
    }
    public interface INaturezaOperacaoService:IBaseService<NaturezaOperacao>
    {
        Task<int> CreateAsync(NaturezaOperacaoCreateRequest request);
        Task UpdateAsync(int id, NaturezaOperacaoUpdateRequest request);
        Task DeleteAsync(int id);
        Task<NaturezaOperacaoResponse?> GetByIdAsync(int id);
        Task<List<NaturezaOperacaoResponse>> GetAllAsync();
        Task<object> GerarPayloadFiscalAsync(int id);
    }
}