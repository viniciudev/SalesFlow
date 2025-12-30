using Model.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO.BoxDto
{
    public class BoxStatus
    {
        public Box CaixaAberto { get; set; }
        public decimal TotalAbertoHoje { get; set; }
        public decimal TotalFechadoHoje { get; set; }
    }
}
