

using System.ComponentModel.DataAnnotations;

namespace Model.Registrations
{
    public class NaturezaOperacao:BaseEntity
    {
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        [Required]
        [MaxLength(150)]
        public string Descricao { get; set; } = null!;

        [Required]
        [MaxLength(10)]
        public string Cfop { get; set; } = null!;

        public Enums.TipoDocumentoEnum TipoDocumento { get; set; }

        public Enums.FinalidadeEnum Finalidade { get; set; }

        public bool ConsumidorFinal { get; set; }

        public bool MovimentaEstoque { get; set; }

        public bool Ativo { get; set; } = true;

        public ConfiguracaoTributaria ConfiguracaoTributaria { get; set; } = new();
    }
}