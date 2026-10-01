using System.ComponentModel.DataAnnotations;

namespace Model.MDFe
{
    /// <summary>
    /// Condutor do veículo no manifesto (<c>infModal.rodo.infCondutor</c>).
    ///
    /// RM06 exige ao menos um condutor com nome e CPF válidos. O leiaute aceita
    /// vários, e a lista é do MANIFESTO e não do veículo: o mesmo caminhão troca
    /// de motorista entre viagens, então amarrar condutor a veículo (RV13) seria
    /// modelar errado.
    ///
    /// Nome e CPF são gravados aqui como snapshot, e não lidos do parceiro na
    /// hora de montar o XML: o condutor é identificado pelo documento no momento
    /// da fiscalização, e o que vale é o que foi declarado. É o único ponto do
    /// manifesto onde o snapshot é o comportamento correto.
    /// </summary>
    public class MdfeCondutor : BaseEntity
    {
        public int IdMdfe { get; set; }
        public MdfeEmissao Mdfe { get; set; }

        /// <summary>
        /// Parceiro do cadastro que corresponde ao condutor, quando houver.
        /// Nulo quando o motorista é avulso (não cadastrado) — o manifesto não
        /// pode depender de o motorista estar no cadastro para ser emitido.
        /// </summary>
        public int? IdClient { get; set; }
        public Client? Client { get; set; }

        /// <summary>Nome completo, como declarado (mínimo 2 palavras — RM06).</summary>
        [Required]
        [StringLength(60)]
        public string Nome { get; set; }

        /// <summary>CPF somente dígitos (11), validado pelos dígitos verificadores (RM06).</summary>
        [Required]
        [StringLength(11)]
        public string Cpf { get; set; }
    }
}
