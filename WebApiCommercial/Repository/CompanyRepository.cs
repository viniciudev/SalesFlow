using Model.Registrations;

namespace Repository
{
  public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
  {
    public CompanyRepository(ContextBase dbContext) : base(dbContext)
    {

    }

  }
  public interface ICompanyRepository : IGenericRepository<Company>
  {

  }

}
