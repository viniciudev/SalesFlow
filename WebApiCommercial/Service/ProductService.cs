using Model;
using Model.DTO;
using Model.Moves;
using Model.Registrations;
using Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service
{
	public class ProductService : BaseService<Product>, IProductService
	{
		private readonly IStockService _stockService;
		private readonly ITributacaoResolverService _tributacaoResolver;

		public ProductService(
				IGenericRepository<Product> repository,
				IStockService stockService,
				ITributacaoResolverService tributacaoResolver) : base(repository)
		{
			_stockService = stockService;
			_tributacaoResolver = tributacaoResolver;
		}

		public async Task<PagedResult<Product>> GetAllPaged(Filters filter)
		{
			return await (repository as IProductRepository).GetAllPaged(filter);
		}
		public async Task<List<Product>> GetListByName(Filters filters)
		{
			return await (repository as IProductRepository).GetListByName(filters);
		}
		public async Task SaveProduct(ProductCreateModelDto model, int tenantid)
		{
			var product = new Product
			{
				Id = model.Id,
				IdCompany = tenantid,
				Name = model.Name,
				Value = model.Value,
				Description = model.Description,
				Quantity = model.Quantity,
				Image = null,
				Code = model.Code,
				Reference = model.Reference,
				CostPrice = model.CostPrice,
				Observation = model.Observation,
				Ncm = model.Ncm,
				Cest = model.Cest,
				// Tributacao por produto
				UsaTributacaoPropria = model.UsaTributacaoPropria,
				NaturezaOperacaoOrigemId = model.NaturezaOperacaoOrigemId,
				DataAtualizacaoTributaria = model.UsaTributacaoPropria ? DateTime.UtcNow : null,

				// Matriz tributaria
				SituacaoTributariaId = model.SituacaoTributariaId,
			};

			// Mapear DTO de tributacao para entidade se estiver preenchido
			if (model.UsaTributacaoPropria && model.ConfiguracaoTributaria != null)
			{
				product.ConfiguracaoTributaria = MapTributacaoDtoToEntity(model.ConfiguracaoTributaria);
			}

			if (product.Id > 0)
			{
				await base.Alter(product);
				// Invalidar cache apos alteracao
				_tributacaoResolver.InvalidarCacheProduto(product.Id);
			}
			else
			{
				await base.Save(product);
				await _stockService.Create(new Stock
				{
					IdCompany = tenantid,
					Quantity = product.Quantity,
					Date = DateTime.UtcNow,
					IdProduct = product.Id,
					Reason = $"Lancamento novo produto: dia {DateTime.UtcNow}",
					Type = StockType.entry
				});
			}
		}

		/// <summary>
		/// Atualiza apenas a configuracao tributaria de um produto existente
		/// </summary>
		public async Task AtualizarTributacaoProdutoAsync(int productId, ProductTributacaoDto dto, int? naturezaOrigemId)
		{
			var product = await base.GetByIdAsync(productId);
			if (product == null)
				throw new InvalidOperationException("Produto nao encontrado.");

			product.UsaTributacaoPropria = true;
			product.ConfiguracaoTributaria = MapTributacaoDtoToEntity(dto);
			product.NaturezaOperacaoOrigemId = naturezaOrigemId;
			product.DataAtualizacaoTributaria = DateTime.UtcNow;

			await base.Alter(product);
			_tributacaoResolver.InvalidarCacheProduto(productId);
		}

		/// <summary>
		/// Remove a tributacao propria do produto (volta a herdar da natureza)
		/// </summary>
		public async Task RemoverTributacaoProdutoAsync(int productId)
		{
			var product = await base.GetByIdAsync(productId);
			if (product == null)
				throw new InvalidOperationException("Produto nao encontrado.");

			product.UsaTributacaoPropria = false;
			product.ConfiguracaoTributaria = null;
			product.NaturezaOperacaoOrigemId = null;
			product.DataAtualizacaoTributaria = DateTime.UtcNow;

			await base.Alter(product);
			_tributacaoResolver.InvalidarCacheProduto(productId);
		}

		private static ConfiguracaoTributaria MapTributacaoDtoToEntity(ProductTributacaoDto dto)
		{
			return new ConfiguracaoTributaria
			{
				AplicarICMS = dto.AplicarICMS,
				CstICMS = dto.CstICMS,
				CsosnICMS = dto.CsosnICMS,
				AliquotaICMS = dto.AliquotaICMS,
				ReduzirBaseICMS = dto.ReduzirBaseICMS,
				AplicarIPI = dto.AplicarIPI,
				CstIPI = dto.CstIPI,
				AliquotaIPI = dto.AliquotaIPI,
				AplicarPIS = dto.AplicarPIS,
				CstPIS = dto.CstPIS,
				AliquotaPIS = dto.AliquotaPIS,
				AplicarCOFINS = dto.AplicarCOFINS,
				CstCOFINS = dto.CstCOFINS,
				AliquotaCOFINS = dto.AliquotaCOFINS,
				AplicarISSQN = dto.AplicarISSQN,
				AliquotaISSQN = dto.AliquotaISSQN,
				AplicarIBS = dto.AplicarIBS,
				CstIBS = dto.CstIBS,
				AliquotaIBS = dto.AliquotaIBS,
				AplicarCBS = dto.AplicarCBS,
				CstCBS = dto.CstCBS,
				AliquotaCBS = dto.AliquotaCBS,
				AplicarIS = dto.AplicarIS,
				AliquotaIS = dto.AliquotaIS,
				cClassTrib = dto.cClassTrib,
			};
		}
	}
	public interface IProductService : IBaseService<Product>
	{
		Task<PagedResult<Product>> GetAllPaged(Filters filter);
		Task<List<Product>> GetListByName(Filters filters);
		Task SaveProduct(ProductCreateModelDto model, int tenantid);
		Task AtualizarTributacaoProdutoAsync(int productId, ProductTributacaoDto dto, int? naturezaOrigemId);
		Task RemoverTributacaoProdutoAsync(int productId);
	}
}
