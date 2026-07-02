using Microsoft.Extensions.Caching.Memory;
using Model.Registrations;
using Repository;
using Service.Exceptions;
using System;
using System.Threading.Tasks;

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

        public async Task<TributacaoResolvida> ResolverTributacaoAsync(int? productId, int naturezaOperacaoId)
        {
            if (!productId.HasValue || productId.Value <= 0)
            {
                var naturezaFallback = await GetNaturezaOperacaoCachedAsync(naturezaOperacaoId);
                return new TributacaoResolvida
                {
                    Configuracao = naturezaFallback.ConfiguracaoTributaria,
                    Origem = "NaturezaOperacao",
                    NaturezaOperacaoOrigemId = naturezaOperacaoId
                };
            }

            var productTask = GetProductCachedAsync(productId.Value);
            var naturezaTask = GetNaturezaOperacaoCachedAsync(naturezaOperacaoId);
            await Task.WhenAll(productTask, naturezaTask);

            var product = productTask.Result;
            var natureza = naturezaTask.Result;

            if (natureza == null)
                throw new DomainException($"Natureza de operacao {naturezaOperacaoId} nao encontrada.");

            if (product == null)
            {
                return new TributacaoResolvida
                {
                    Configuracao = natureza.ConfiguracaoTributaria,
                    Origem = "NaturezaOperacao",
                    NaturezaOperacaoOrigemId = naturezaOperacaoId
                };
            }

            if (product.UsaTributacaoPropria
                && natureza.PermiteTributacaoPorProduto
                && product.ConfiguracaoTributaria != null)
            {
                return new TributacaoResolvida
                {
                    Configuracao = product.ConfiguracaoTributaria,
                    Origem = "Produto",
                    NaturezaOperacaoOrigemId = product.NaturezaOperacaoOrigemId
                };
            }

            return new TributacaoResolvida
            {
                Configuracao = natureza.ConfiguracaoTributaria,
                Origem = "NaturezaOperacao",
                NaturezaOperacaoOrigemId = naturezaOperacaoId
            };
        }

        public async Task<ConfiguracaoTributaria> ClonarDeNaturezaAsync(int naturezaOperacaoId)
        {
            var natureza = await GetNaturezaOperacaoCachedAsync(naturezaOperacaoId);
            if (natureza == null)
                throw new DomainException($"Natureza de operacao {naturezaOperacaoId} nao encontrada.");
            return CloneConfiguracao(natureza.ConfiguracaoTributaria);
        }

        public ConfiguracaoTributaria CloneConfiguracao(ConfiguracaoTributaria origem)
        {
            if (origem == null) return new ConfiguracaoTributaria();
            return new ConfiguracaoTributaria
            {
                AplicarICMS = origem.AplicarICMS,
                CstICMS = origem.CstICMS,
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
            };
        }

        private async Task<Product?> GetProductCachedAsync(int productId)
        {
            var cacheKey = $"product_trib_{productId}";
            if (_cache.TryGetValue(cacheKey, out Product? cached))
                return cached;
            var product = await _productRepository.GetByIdAsync(productId);
            if (product != null)
                _cache.Set(cacheKey, product, _cacheOptions);
            return product;
        }

        private async Task<NaturezaOperacao?> GetNaturezaOperacaoCachedAsync(int naturezaId)
        {
            var cacheKey = $"natop_trib_{naturezaId}";
            if (_cache.TryGetValue(cacheKey, out NaturezaOperacao? cached))
                return cached;
            var natureza = await _naturezaOperacaoRepository.GetById(naturezaId);
            if (natureza != null)
                _cache.Set(cacheKey, natureza, _cacheOptions);
            return natureza;
        }

        public void InvalidarCacheProduto(int productId)
        {
            _cache.Remove($"product_trib_{productId}");
        }

        public void InvalidarCacheNatureza(int naturezaId)
        {
            _cache.Remove($"natop_trib_{naturezaId}");
        }
    }

    public class TributacaoResolvida
    {
        public ConfiguracaoTributaria Configuracao { get; set; } = new();
        public string Origem { get; set; } = "NaturezaOperacao";
        public int? NaturezaOperacaoOrigemId { get; set; }
    }

    public interface ITributacaoResolverService
    {
        Task<TributacaoResolvida> ResolverTributacaoAsync(int? productId, int naturezaOperacaoId);
        Task<ConfiguracaoTributaria> ClonarDeNaturezaAsync(int naturezaOperacaoId);
        ConfiguracaoTributaria CloneConfiguracao(ConfiguracaoTributaria origem);
        void InvalidarCacheProduto(int productId);
        void InvalidarCacheNatureza(int naturezaId);
    }
}
