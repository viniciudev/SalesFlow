using Model.Registrations;
using Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service
{
   public class CompanyService : BaseService<Company>, ICompanyService
   {
      public CompanyService(IGenericRepository<Company> repository) : base(repository)
      {
      }
     

   }
   public interface ICompanyService : IBaseService<Company>
   {
   }
}
