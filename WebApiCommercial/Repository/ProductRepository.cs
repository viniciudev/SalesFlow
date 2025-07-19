﻿using Microsoft.EntityFrameworkCore;
using Model;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
  public class ProductRepository : GenericRepository<Product>, IProductRepository
  {
    public ProductRepository(ContextBase dbContext) : base(dbContext)
    {
    }

    public async Task<PagedResult<Product>> GetAllPaged(Filters filter)
    {
      var paged = await base._dbContext.Set<Product>().Where(x=>x.IdCompany==filter.idCompany)
             .Select(p => new Product {
                 Id= p.Id,
                Name= p.Name,
                 Value = p.Value,
                 Description = p.Description,
                 Quantity = p.Quantity,
                Image = p.Image,
                 ImageBytes = p.Image != null ? Convert.ToBase64String(p.Image) : null
             })
        .GetPagedAsync<Product>(filter.pageNumber, filter.pageSize);
      return paged;
    }
    public async Task<List<Product>> GetListByName(Filters filters)
    {
      var data = await base._dbContext.Set<Product>().Where(x =>
      x.IdCompany == filters.idCompany &&
      (string.IsNullOrEmpty(filters.textOption)|| x.Name.Contains(filters.textOption)))
        .AsNoTracking().ToListAsync();
      return data;
    }
  }
  public interface IProductRepository : IGenericRepository<Product>
  {
    Task<PagedResult<Product>> GetAllPaged(Filters filter);
    Task<List<Product>> GetListByName(Filters filters);
  }
}
