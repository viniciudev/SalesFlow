using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Moves
{
    
    public enum CaixaStatus
    {
        ABERTO,
        FECHADO,
        EM_CONCILIACAO
    }

    public enum CaixaMovimentacaoTipo
    {
        ENTRADA,
        SAIDA,
        SANGRIA,
        SUPRIMENTO
    }

    public class Box:BaseEntity
    {

        public int? UsuarioId { get; set; }
        public DateTime DataAbertura { get; set; }
        public DateTime? DataFechamento { get; set; }
        public decimal ValorInicial { get; set; }
        public decimal? ValorFinal { get; set; }
        public decimal? SaldoCalculado { get; set; }
        public decimal? Diferenca { get; set; }
        public CaixaStatus Status { get; set; }
        public string? Observacoes { get; set; }
        public DateTime CreatedAt { get; set; }
        // Navigation properties
        public  ICollection<Financial> Movimentacoes { get; set; } = new List<Financial>();
    }
}
