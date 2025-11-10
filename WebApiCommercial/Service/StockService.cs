using Model.Moves;
using Repository;

namespace Service
{


    public class StockService : BaseService<Stock>, IStockService
    {
        private ICompanyService companyService;
        private IPlanCompanyService planCompanyService;
        private ICostCenterService costCenterService;
        private IEmailService emailService;
        public StockService(IGenericRepository<Stock> repository,
          ICompanyService companyService,
          IPlanCompanyService planCompanyService,
          ICostCenterService costCenterService,
          IEmailService emailService) : base(repository)

        {
            this.companyService = companyService;
            this.planCompanyService = planCompanyService;
            this.costCenterService = costCenterService;
            this.emailService = emailService;
        }
        //public Task<Stock> GetStock(AuthenticateModel model)
        //{
        //    return (repository as IStockRepository).GetStock(model);
        //}
    }
    public interface IStockService : IBaseService<Stock>
    {
    }
}
