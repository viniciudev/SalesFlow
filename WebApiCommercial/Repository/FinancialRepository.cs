using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO;
using Model.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
  public class FinancialRepository : GenericRepository<Financial>, IFinancialRepository
  {
    public FinancialRepository(ContextBase dbContext) : base(dbContext)
    {
    }
    public async Task<List<Financial>> SearchBySaleItemsId(int id,TypeItem typeItem,int idItem)
    {
      var data = await _dbContext.Set<Financial>().Where(
        x => x.IdSaleItems == id
        && (typeItem==TypeItem.Product && x.IdProduct==idItem
        ||typeItem==TypeItem.Service && x.IdService==idItem)
        ).AsNoTracking().ToListAsync();
      return data;
    }
    public async Task<PagedResult<CommissionFinancialResponse>> GetPagedByFilter(Filters filters)
    {
      try
      {
        var data = await (from fin in _dbContext.Set<Financial>()
                  .Include(x => x.Sale).ThenInclude(x=>x.Client)
                  .Include(x => x.Product)
                  .Include(x => x.ServiceProvided)
                          where (fin.IdSalesman ==0|| fin.IdSalesman==filters.IdSeller)
                          && (fin.CreationDate >= filters.SaleDate.Date
                              && fin.CreationDate <= filters.SaleDateFinal.Date)
                          select new CommissionFinancialResponse
                          {
                            IdFinancial = fin.Id,
                            ReleaseDate = fin.CreationDate,
                            DateSale = fin.Sale.ReleaseDate,
                            Item = fin.Product != null ? fin.Product.Name : fin.ServiceProvided.Name,
                            Percentage = fin.Percentage,
                            Value = fin.Value,
                            DueDate=fin.DueDate,
                            Origin=fin.Sale.Client.Name
                          }).AsNoTracking()
                   .GetPagedAsync<CommissionFinancialResponse>(filters.pageNumber, filters.pageSize);

        return data;
      }
      catch (System.Exception ex)
      {
        throw;
      }
    }
    public async Task<List<Financial>> GetByIdCompany(Filters filters)
    {
      try
      {
        var data = await (from fin in _dbContext.Set<Financial>()
             
                          where (fin.IdCompany == filters.idCompany)
                          &&(fin.Origin==OriginFinancial.financial)
                          && (fin.CreationDate >= filters.SaleDate.Date
                              && fin.CreationDate <= filters.SaleDateFinal.Date)
                          select new Financial
                          {
                            Id = fin.Id,
                            Value = fin.Value,
                            DueDate = fin.DueDate,
                            Description=fin.Description,
                            FinancialType= fin.FinancialType,
                            PaymentType= fin.PaymentType,
                            CreationDate= fin.CreationDate,
                          }).AsNoTracking()
                   .GetPagedAsync<Financial>(filters.pageNumber, filters.pageSize);

        return data.Results.ToList();
      }
      catch (System.Exception ex)
      {
        throw;
      }
    }
    public async Task<CommissionInfoResponse> GetByMonthAllCommission(Filters filters)
    {
      try
      {
        CommissionInfoResponse clientInfoResponse = new CommissionInfoResponse { AmountMonth = new List<int>(), CommissionAmount = 0 };
        List<Financial> data = await _dbContext.Set<Financial>()
          .Where(x => x.IdCompany == filters.idCompany
          && x.CreationDate.Year == DateTime.Now.Year
          && x.commission==true)
          .AsNoTracking().ToListAsync();

        var grupo1 = data.GroupBy(c => c.CreationDate.Month)
                                  .Select(g => new { Key = g.Key, Itens = g.ToList() }).ToList();

        for (int i = 1; i < 13; i++)
        {
          var teste = grupo1.FirstOrDefault(x => x.Key == i);
          if (teste != null && teste.Key > 0)
            clientInfoResponse.AmountMonth.Add(teste.Itens.Count);
          else
            clientInfoResponse.AmountMonth.Add(0);
        }
        clientInfoResponse.CommissionAmount = data.Count();
        return clientInfoResponse;
      }
      catch (System.Exception ex)
      {

        throw;
      }
    }
  }
  public interface IFinancialRepository : IGenericRepository<Financial>
  {
    Task<List<Financial>> SearchBySaleItemsId(int id,TypeItem typeItem, int idItem);
    Task<PagedResult<CommissionFinancialResponse>> GetPagedByFilter(Filters filters);
    Task<CommissionInfoResponse> GetByMonthAllCommission(Filters filters);
    Task<List<Financial>> GetByIdCompany(Filters filters);
  }
}
