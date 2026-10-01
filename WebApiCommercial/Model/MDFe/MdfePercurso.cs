using System.ComponentModel.DataAnnotations;

namespace Model.MDFe
{
    /// <summary>
    /// Uma UF por onde a carga passa antes de chegar ao destino
    /// (<c>infMDFe.infPercurso</c>).
    ///
    /// O leiaute permite até 25 UFs de percurso, e a ORDEM importa: os XSDs
    /// definem a sequência como a ordem de passagem, então <see cref="Ordem"/> é
    /// gravada e o XML é montado por ela — não pela ordem de inserção no banco,
    /// que pode mudar.
    ///
    /// RM13 valida que as UFs existem em <see cref="UfList"/>, não se repetem e
    /// não repetem a UF de carregamento nem a de descarregamento (incluir o
    /// próprio trajeto já declarado é erro de digitação, não intenção).
    /// </summary>
    public class MdfePercurso : BaseEntity
    {
        public int IdMdfe { get; set; }
        public MdfeEmissao Mdfe { get; set; }

        /// <summary>Sigla da UF (2 letras). RM12/RM13 validam contra <see cref="UfList"/>.</summary>
        [Required]
        [StringLength(2)]
        public string UfPercurso { get; set; }

        /// <summary>Posição na sequência do trajeto, começando em 1.</summary>
        public int Ordem { get; set; }
    }
}
