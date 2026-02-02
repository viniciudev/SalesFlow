using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Moves
{
    public class FinancialResources : BaseEntity
    {
        public int IdRefOrigin { get; set; }
     
        public int IdNewFinancial { get; set; }
    }
}
