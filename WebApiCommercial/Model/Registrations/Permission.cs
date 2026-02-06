using Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Registrations
{
    public class Permission:BaseEntity
    {
    
        public string Name { get; set; }
        public PermissionEnum Code { get; set; } // Ex: "CADASTRO_EMPRESA", "VENDA_CREATE"
        public string Description { get; set; }
        public ICollection<UserPermission> UserPermissions { get; set; }
    }
}
