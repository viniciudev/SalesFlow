using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.Registrations
{
    /// <summary>
    /// Catalogo de situacoes tributarias do produto.
    /// Exemplos: NORMAL, ST_RETIDA, SUBSTITUIDA, ISENTA, etc.
    /// </summary>
    public class SituacaoTributaria : BaseEntity
    {
        public int CompanyId { get; set; }
        public Company Company { get; set; }

        [Required]
        [MaxLength(30)]
        public string Codigo { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Descricao { get; set; } = null!;

        public ICollection<RegraFiscal> RegrasFiscais { get; set; } = new List<RegraFiscal>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
