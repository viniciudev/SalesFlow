using Model;
using Model.Registrations;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
 
    public class PermissionService : BaseService<Permission>, IPermissionService
    {
        public PermissionService(IGenericRepository<Permission> repository) : base(repository)
        {
        }
    }
        public interface IPermissionService : IBaseService<Permission>
    {
    }
    }
