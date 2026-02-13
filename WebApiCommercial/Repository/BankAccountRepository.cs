using Model;
using Model.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
  
    public class BankAccountRepository : GenericRepository<BankAccount>, IBankAccountRepository
    {
        public BankAccountRepository(ContextBase dbContext) : base(dbContext)
        {

        }
    }
        public interface IBankAccountRepository : IGenericRepository<BankAccount>
    {
    }
    }
