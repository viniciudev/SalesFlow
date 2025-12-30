using Model.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO.BoxDto
{
    public class BoxDto
    {
        public int Id { get; set; }
        public DateTime DataAbertura { get; set; }
        public DateTime? DataFechamento { get; set; }
        public decimal ValorInicial { get; set; }
        public decimal? ValorFinal { get; set; }
        public decimal? SaldoCalculado { get; set; }
        public decimal? Diferenca { get; set; }
        public CaixaStatus Status { get; set; }
        public string? Observacoes { get; set; }
    }
}
