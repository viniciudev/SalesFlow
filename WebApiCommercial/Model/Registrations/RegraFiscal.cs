using Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace Model.Registrations
{
    /// <summary>
    /// Matriz tributaria: representa uma celula da combinacao
    /// NaturezaOperacao + SituacaoTributaria + Destino.
    /// Substitui o antigo modelo de ConfiguracaoTributaria unica por natureza,
    /// permitindo CFOP e tributos diferentes por situacao e destino.
    ///
    /// Reutiliza ConfiguracaoTributaria como owned type para os campos de tributos.
    /// </summary>
    public class RegraFiscal : BaseEntity
    {
        public int NaturezaOperacaoId { get; set; }
        public NaturezaOperacao NaturezaOperacao { get; set; } = null!;

        public int SituacaoTributariaId { get; set; }
        public SituacaoTributaria SituacaoTributaria { get; set; } = null!;

        /// <summary>
        /// Destino da operacao: Interno, Interestadual ou Exterior.
        /// </summary>
        public Destino Destino { get; set; }

        /// <summary>
        /// CFOP especifico para esta combinacao.
        /// Ex: 5102 para Normal/Interno, 5405 para ST/Interno, 6102 para Normal/Interestadual.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string Cfop { get; set; } = null!;

        /// <summary>
        /// Configuracao tributaria (owned type) com todos os tributos.
        /// Reutiliza a estrutura existente de ConfiguracaoTributaria.
        /// </summary>
        public ConfiguracaoTributaria ConfiguracaoTributaria { get; set; } = new();
    }
}
