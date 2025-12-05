using GoogleApi.Entities.Search.Video.Common;
using Model.DTO;
using Repository;
using System.Threading.Tasks;

namespace Service
{


    public class DashboardService : IDashboardService
    {
        private ISaleRepository _saleRepository;
        private IClientRepository _clientRepository;
        private IStockRepository _stockRepository;
        private IProductRepository _productRepository;
        public DashboardService(ISaleRepository saleRepository, IClientRepository clientRepository, IStockRepository stockRepository, IProductRepository productRepository)
        {
            _saleRepository = saleRepository;
            _clientRepository = clientRepository;
            _stockRepository = stockRepository;
            _productRepository = productRepository;
        }

        public async Task<MonthlySalesComparisonResult> GetValeuSalesByIdCompany(int id)
        {
            return await _saleRepository.GetMonthlySalesWithComparisonByIdCompany(id);
        }
        public async Task<MonthlyClientsComparisonResult> GetAmountClientByIdCompany(int tenantid)
        {
            return await _clientRepository.GetMonthlyClientsWithComparisonByIdCompany(tenantid);
        }
        public async Task<int> GetLowStockByIdCompany(int tenantid)
        {
            return await _stockRepository.GetLowStockByIdCompany(tenantid);
        }
        public async Task<int> GetProductsByIdCompany(int tenantid)
        {
            return await _productRepository.GetCountProductsByIdCompany(tenantid);
        }
    }
    public interface IDashboardService
    {
        Task<MonthlyClientsComparisonResult> GetAmountClientByIdCompany(int tenantid);
        Task<int> GetLowStockByIdCompany(int tenantid);
        Task<int> GetProductsByIdCompany(int tenantid);
        Task<MonthlySalesComparisonResult> GetValeuSalesByIdCompany(int id);
    }
}
