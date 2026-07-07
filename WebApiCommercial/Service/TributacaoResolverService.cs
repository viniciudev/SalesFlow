using Microsoft.Extensions.Caching.Memory;
using Model.Enums;
using Model.Registrations;
using Repository;
using Service.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Service
{
    public class TributacaoResolverService : ITributacaoResolverService
    {
        private readonly IProductRepository _productRepository;
        private readonly INaturezaOperacaoRepository _naturezaOperacaoRepository;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public TributacaoResolverService(
            IProductRepository productRepository,
            INaturezaOperacaoRepository naturezaOperacaoRepository,
            IMemoryCache cache)
        {
            _productRepository = productRepository;
            _naturezaOperacaoRepository = naturezaOperacaoRepository;
            _cache = cache;
            _cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                .SetSize(1);
        }

        public Destino ResolverDestino(string empresaUf, string clienteUf, string? clienteCodPais)
        {
            if (!string.IsNullOrEmpty(clienteCodPais) && clienteCodPais != "1058")
                return Destino.Exterior;

            if (string.IsNullOrWhiteSpace(empresaUf) || string.IsNullOrWhiteSpace(clienteUf))
                return Destino.Interno;

            if (string.Equals(empresaUf.Trim(), clienteUf.Trim(), StringComparison.OrdinalIgnoreCase))
                return Destino.Interno;

            return Destino.Interestadual;
        }

        public async Task<RegraFiscal?> ResolverRegraFiscalAsync(
            int naturezaOperacaoId,
            int? situacaoTributariaId,
            Destino destino)
        {
            if (!situacaoTributariaId.HasValue || situacaoTributariaId.Value <= 0)
                return null;

            var natureza = await GetNaturezaOperacaoCachedAsync(naturezaOperacaoId);
            if (natureza == null)
                return null;

            var regra = natureza.RegrasFiscais?
                .FirstOrDefault(r =>
                    r.SituacaoTributariaId == situacaoTributariaId.Value &&
                    r.Destino == destino);

            return regra;
        }

        public async Task<TributacaoResolvida> ResolverTributacaoAsync(
            int? productId,
            int naturezaOperacaoId,
            string? empresaUf = null,
            string? clienteUf = null,
            string? clienteCodPais = null)
        {
            var natureza = await _naturezaOperacaoRepository.GetByIdWithRegrasAsync (naturezaOperacaoId);
            var product =  productId.HasValue && productId.Value > 0
                ? await _productRepository.GetByIdAsync(productId.Value) : null;

            //await Task.WhenAll(
            //    naturezaTask,
            //    productTask ?? Task.CompletedTask);

            //var natureza = naturezaTask.Result;
            //var product = productTask?.Result;

            if (natureza == null)
                throw new DomainException("Natureza de operacao nao encontrada.");

            if (product != null && product.SituacaoTributariaId.HasValue && product.SituacaoTributariaId > 0
                && !string.IsNullOrWhiteSpace(empresaUf) && !string.IsNullOrWhiteSpace(clienteUf))
            {
                var destino = ResolverDestino(empresaUf, clienteUf, clienteCodPais ?? "1058");
                var regraFiscal = natureza.RegrasFiscais?
                    .FirstOrDefault(r =>
                        r.SituacaoTributariaId == product.SituacaoTributariaId.Value &&
                        r.Destino == destino);

                if (regraFiscal != null)
                {
                    return new TributacaoResolvida
                    {
                        Configuracao = regraFiscal.ConfiguracaoTributaria ?? new ConfiguracaoTributaria(),
                        Cfop = regraFiscal.Cfop,
                        Origem = "MatrizTributaria",
                        NaturezaOperacaoOrigemId = natureza.Id,
                        RegraFiscalId = regraFiscal.Id,
                        Destino = destino
                    };
                }

                regraFiscal = natureza.RegrasFiscais?
                    .FirstOrDefault(r =>
                        r.SituacaoTributariaId == product.SituacaoTributariaId.Value &&
                        r.Destino == Destino.Interno);

                if (regraFiscal != null)
                {
                    return new TributacaoResolvida
                    {
                        Configuracao = regraFiscal.ConfiguracaoTributaria ?? new ConfiguracaoTributaria(),
                        Cfop = regraFiscal.Cfop,
                        Origem = "MatrizTributaria",
                        NaturezaOperacaoOrigemId = natureza.Id,
                        RegraFiscalId = regraFiscal.Id,
                        Destino = destino
                    };
                }
            }

            if (product != null
                && product.UsaTributacaoPropria
                && natureza.PermiteTributacaoPorProduto
                && product.ConfiguracaoTributaria != null)
            {
                return new TributacaoResolvida
                {
                    Configuracao = product.ConfiguracaoTributaria,
                    Cfop = natureza.Cfop,
                    Origem = "Produto",
                    NaturezaOperacaoOrigemId = product.NaturezaOperacaoOrigemId
                };
            }

            return new TributacaoResolvida
            {
                Configuracao = natureza.ConfiguracaoTributaria ?? new ConfiguracaoTributaria(),
                Cfop = natureza.Cfop,
                Origem = "NaturezaOperacao",
                NaturezaOperacaoOrigemId = naturezaOperacaoId
            };
        }

        public async Task<TributacaoResolvida> ResolverTributacaoAsync(int? productId, int naturezaOperacaoId)
        {
            return await ResolverTributacaoAsync(productId, naturezaOperacaoId, null, null, null);
        }

        public async Task<ConfiguracaoTributaria> ClonarDeNaturezaAsync(int naturezaOperacaoId)
        {
            var natureza = await GetNaturezaOperacaoCachedAsync(naturezaOperacaoId);
            if (natureza == null)
                throw new DomainException("Natureza de operacao nao encontrada.");
            return CloneConfiguracao(natureza.ConfiguracaoTributaria);
        }

        public ConfiguracaoTributaria CloneConfiguracao(ConfiguracaoTributaria origem)
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

        private async Task<Product?> GetProductCachedAsync(int productId)
        {
            var cacheKey = "product_trib_" + productId;
            if (_cache.TryGetValue(cacheKey, out Product? cached))
                return cached;
            var product = await _productRepository.GetByIdAsync(productId);
            if (product != null)
                _cache.Set(cacheKey, product, _cacheOptions);
            return product;
        }

        private async Task<NaturezaOperacao?> GetNaturezaOperacaoCachedAsync(int naturezaId)
        {
            var cacheKey = "natop_trib_" + naturezaId;
            if (_cache.TryGetValue(cacheKey, out NaturezaOperacao? cached))
                return cached;
            var natureza = await _naturezaOperacaoRepository.GetByIdWithRegrasAsync(naturezaId);
            if (natureza != null)
                _cache.Set(cacheKey, natureza, _cacheOptions);
            return natureza;
        }

        public void InvalidarCacheProduto(int productId)
        {
            _cache.Remove("product_trib_" + productId);
        }

        public void InvalidarCacheNatureza(int naturezaId)
        {
            _cache.Remove("natop_trib_" + naturezaId);
        }
    }

    public class TributacaoResolvida
    {
        public ConfiguracaoTributaria Configuracao { get; set; } = new();
        public string Cfop { get; set; } = string.Empty;
        public string Origem { get; set; } = "NaturezaOperacao";
        public int? NaturezaOperacaoOrigemId { get; set; }
        public int? RegraFiscalId { get; set; }
        public Destino? Destino { get; set; }
    }

    public interface ITributacaoResolverService
    {
        Task<TributacaoResolvida> ResolverTributacaoAsync(int? productId, int naturezaOperacaoId);
        Task<TributacaoResolvida> ResolverTributacaoAsync(int? productId, int naturezaOperacaoId,
            string? empresaUf, string? clienteUf, string? clienteCodPais);
        Destino ResolverDestino(string empresaUf, string clienteUf, string? clienteCodPais);
        Task<RegraFiscal?> ResolverRegraFiscalAsync(int naturezaOperacaoId, int? situacaoTributariaId, Destino destino);
        Task<ConfiguracaoTributaria> ClonarDeNaturezaAsync(int naturezaOperacaoId);
        ConfiguracaoTributaria CloneConfiguracao(ConfiguracaoTributaria origem);
        void InvalidarCacheProduto(int productId);
        void InvalidarCacheNatureza(int naturezaId);
    }
}
