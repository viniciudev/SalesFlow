using Model;
using Model.Registrations;
using Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service
{
    public class SalesmanService : BaseService<Salesman>, ISalesmanService
    {
        public SalesmanService(IGenericRepository<Salesman> repository) : base(repository)
        {
        }

        public async Task<PagedResult<Salesman>> GetAllPaged(Filters filter)
        {
            return await (repository as ISalesmanRepository).GetAllPaged(filter);
        }
        public async Task SaveSalesman(Salesman salesman)
        {
            if (salesman.Id > 0)
            {
                await base.Alter(salesman);
            }
            else
            {
                await base.Save(salesman);
            }
        }
        public async Task<List<Salesman>> GetListByName(Filters filters)
        {
            return await (repository as ISalesmanRepository).GetListByName(filters);
        }
        public async Task<List<Salesman>> GetAllList(Filters filters)
        {
            return await (repository as ISalesmanRepository).GetAllList(filters);
        }
        public async Task<List<Salesman>> GetAllByGuid(string guid)
        {
            return await (repository as ISalesmanRepository).GetAllByGuid(guid);
        }
    }
    public interface ISalesmanService : IBaseService<Salesman>
    {
        Task<PagedResult<Salesman>> GetAllPaged(Filters filter);
        Task SaveSalesman(Salesman salesman);
        Task<List<Salesman>> GetListByName(Filters filters);
        Task<List<Salesman>> GetAllList(Filters filters);
        Task<List<Salesman>> GetAllByGuid(string guid);
    }
}
