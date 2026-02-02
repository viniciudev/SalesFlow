using GoogleApi.Entities.Search.Common;
using GoogleApi.Entities.Search.Video.Common;
using Model;
using Model.DTO;
using Model.Moves;
using Model.Registrations;
using Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Service
{
    public class ProductService : BaseService<Product>, IProductService
    {
        private readonly IStockService _stockService;
        public ProductService(IGenericRepository<Product> repository, IStockService stockService) : base(repository)
        {
            _stockService = stockService;
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

            //byte[] file = null;
            //if (model.Id > 0)
            //{
            //    var existingProduct = await base.GetByIdAsync(model.Id);
            //    file = existingProduct.Image;
            //}

            //// Processar a imagem se existir
            //if (model.Image != null && model.Image.Length > 0)
            //{
            //    // Obter array de bytes da imagem
            //    using (var memoryStream = new MemoryStream())
            //    {
            //        await model.Image.CopyToAsync(memoryStream);
            //        byte[] imageBytes = memoryStream.ToArray();

            //        // Aqui você pode:
            //        // 1. Salvar o array de bytes no banco de dados (se sua entidade tiver essa propriedade)
            //        // product.ImageBytes = imageBytes;

            //        // 2. Ou salvar no sistema de arquivos como no exemplo anterior
            //        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            //        if (!Directory.Exists(uploadsFolder))
            //        {
            //            Directory.CreateDirectory(uploadsFolder);
            //        }

            //        var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
            //        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            //        // Salvar bytes no arquivo
            //        await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
            //        file = imageBytes;
            //        //product.ImageUrl = uniqueFileName; // Ou o caminho completo se preferir
            //    }
            //}
            var product = new Product
            {
                Id = model.Id,
                IdCompany = tenantid,
                Name = model.Name,
                Value = model.Value,
                Description = model.Description,
                Quantity = model.Quantity,
                Image = null,//file
                Code=model.Code,
                Reference=model.Reference,
                CostPrice=model.CostPrice,
                Observation=model.Observation
            };
            if (product.Id > 0)
                await base.Alter(product);
            else
            {
                await base.Save(product);
                await _stockService.Create(new Stock
                {
                    IdCompany = tenantid,
                    Quantity = product.Quantity,
                    Date = DateTime.UtcNow,
                    IdProduct = product.Id,
                    Reason = $"Lançamento novo produto: dia {DateTime.UtcNow}",
                    Type = StockType.entry
                });
            }
                
        }
    }
    public interface IProductService : IBaseService<Product>
    {
        Task<PagedResult<Product>> GetAllPaged(Filters filter);
        Task<List<Product>> GetListByName(Filters filters);
        Task SaveProduct(ProductCreateModelDto model, int tenantid);
    }
}
