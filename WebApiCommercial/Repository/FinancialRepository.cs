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
        public async Task<List<Financial>> SearchBySaleItemsId(int id, TypeItem typeItem, int idItem)
        {
            var data = await _dbContext.Set<Financial>().Where(
              x => x.IdSaleItems == id
              && (typeItem == TypeItem.Product && x.IdProduct == idItem
              || typeItem == TypeItem.Service && x.IdService == idItem)
              ).AsNoTracking().ToListAsync();
            return data;
        }
        public async Task<PagedResult<CommissionFinancialResponse>> GetPagedByFilter(Filters filters)
        {
            try
            {
                var data = await (from fin in _dbContext.Set<Financial>()
                          .Include(x => x.Sale).ThenInclude(x => x.Client)
                          .Include(x => x.Product)
                          .Include(x => x.ServiceProvided)
                                  where (fin.IdSalesman == 0 || fin.IdSalesman == filters.IdSeller)
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
                                      DueDate = fin.DueDate,
                                      Origin = fin.Sale.Client.Name
                                  }).AsNoTracking()
                           .GetPagedAsync<CommissionFinancialResponse>(filters.PageNumber, filters.PageSize);

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

                                  where (fin.IdCompany == filters.IdCompany)
                                  && (fin.Origin == OriginFinancial.financial)
                                  && (fin.CreationDate >= filters.SaleDate.Date
                                      && fin.CreationDate <= filters.SaleDateFinal.Date)
                                  select new Financial
                                  {
                                      Id = fin.Id,
                                      Value = fin.Value,
                                      DueDate = fin.DueDate,
                                      Description = fin.Description,
                                      FinancialType = fin.FinancialType,
                                      PaymentType = fin.PaymentType,
                                      CreationDate = fin.CreationDate,
                                  }).AsNoTracking()
                           .GetPagedAsync<Financial>(filters.PageNumber, filters.PageSize);

                return data.Results.ToList();
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }
        public async Task<PagedResult<FinancialResponse>> GetPaged(Filters filters)
        {
            try
            {
                var data = await (from fin in _dbContext.Set<Financial>()
                          .Include(x => x.Sale).ThenInclude(x => x.Client)
                          .Include(x => x.Product)
                          .Include(x => x.ServiceProvided)
                          .Include(x => x.Client)
                          .Include(x=>x.PaymentMethod)
                                  where
                                  fin.IdCompany == filters.IdCompany
                                  && (string.IsNullOrEmpty(filters.TextOption) || fin.Description.Contains(filters.TextOption))
                                  && (filters.FinancialStatus == null || fin.FinancialStatus == filters.FinancialStatus)
                                  orderby fin.Id descending
                                  select new FinancialResponse
                                  {
                                      Id = fin.Id,
                                      CreationDate = fin.CreationDate,
                                      Value = fin.Value,
                                      DueDate = fin.DueDate,
                                      Origin = fin.Origin,
                                      FinancialStatus = fin.FinancialStatus,
                                      PaymentMethodName = fin.PaymentMethod.Name,
                                      PaymentMethodId=fin.PaymentMethod.Id,
                                      Description = fin.Description,
                                      FinancialType = fin.FinancialType,
                                      IdCompany = fin.IdCompany,
                                      ClientName = fin.Client.Name,
                                      ClientId = fin.Client.Id,
                                      FinancialResourcesResponseList = _dbContext.Set<FinancialResources>()
                            .Where(fr => fr.IdNewFinancial == fin.Id)
                            .Join(_dbContext.Set<Financial>(),
                                  fr => fr.IdRefOrigin,
                                  f => f.Id,
                                  (fr, f) => new { fr, f })
                            .Select(x => new FinancialResourcesResponse
                            {
                                Id = x.f.Id,
                                Description = x.f.Description,
                                Value = x.f.Value
                            })
                            .ToList()

                                  }).AsNoTracking()
                           .GetPagedAsync<FinancialResponse>(filters.PageNumber, filters.PageSize);

                return data;
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
                  .Where(x => x.IdCompany == filters.IdCompany
                  && x.CreationDate.Year == DateTime.Now.Year
                  && x.commission == true)
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
        public async Task<List<Financial>> GetByIdSaleAsync(int id)
        {
            var data = await _dbContext.Set<Financial>()
                .Where(x => x.IdSale == id)
                .AsNoTracking()
                .ToListAsync();
            return data;
        }
        public async Task<PagedResult<Financial>> GetPagedByIdClient(Filters filters)
        {

            try
            {
                var data = await (from fin in _dbContext.Set<Financial>()
                                     .Include(x => x.PaymentMethod)
                                  where (fin.IdCompany == filters.IdCompany)
                                  && (fin.IdClient == filters.IdClient)
                                 && (fin.FinancialStatus == FinancialStatus.pending)
                                 &&(fin.FinancialType== FinancialType.recipe)
                                  select new Financial
                                  {
                                      Id = fin.Id,
                                      Value = fin.Value,
                                      DueDate = fin.DueDate,
                                      Description = fin.Description,
                                      FinancialType = fin.FinancialType,
                                      PaymentMethodName = fin.PaymentMethod.Name,
                                      CreationDate = fin.CreationDate,
                                      FinancialStatus = fin.FinancialStatus,
                                      
                                  }).AsNoTracking()
                           .GetPagedAsync(filters.PageNumber, filters.PageSize);

                return data;
            }
            catch (System.Exception ex)
            {
                throw;
            }

        }
    }
    public interface IFinancialRepository : IGenericRepository<Financial>
    {
        Task<List<Financial>> SearchBySaleItemsId(int id, TypeItem typeItem, int idItem);
        Task<PagedResult<CommissionFinancialResponse>> GetPagedByFilter(Filters filters);
        Task<CommissionInfoResponse> GetByMonthAllCommission(Filters filters);
        Task<List<Financial>> GetByIdCompany(Filters filters);
        Task<List<Financial>> GetByIdSaleAsync(int id);
        Task<PagedResult<FinancialResponse>> GetPaged(Filters filters);
        Task<PagedResult<Financial>> GetPagedByIdClient(Filters filters);

    }
}
