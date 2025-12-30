using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO.BoxDto
{
    public class OpenBoxDto
    {
        public int IdCompany { get; set; }
        public decimal ValorInicial { get; set; }
        public string? Observacoes { get; set; }
    }
}
