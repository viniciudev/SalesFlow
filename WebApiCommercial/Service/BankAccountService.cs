using Model;
using Model.Moves;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
  

    public class BankAccountService : BaseService<BankAccount>, IBankAccountService
    {
        public BankAccountService(IGenericRepository<BankAccount> repository) : base(repository)
        {
        }
    }
        public interface IBankAccountService : IBaseService<BankAccount>
    {
    }
    }
