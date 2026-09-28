using Model.Enums;
using Model.Registrations;
using System.ComponentModel.DataAnnotations;

namespace Model.MDFe
{
    /// <summary>
    /// Veículo escalado no manifesto: exatamente um de tração e de zero a cinco
    /// reboques, conforme o XSD <c>mdfeModalRodoviario_v3.00.xsd</c>.
    ///
    /// Placa, tara, tipo de rodado, tipo de carroceria e UF de licenciamento NÃO
    /// são copiados para cá: eles vêm do <see cref="Vehicle"/> no momento de
    /// montar o XML. Copiar seria pior em dois sentidos — dobraria o cadastro de
    /// veículo dentro do manifesto, e um veículo corrigido depois continuaria
    /// gerando manifesto com o dado velho. O que é snapshot fica em
    /// <see cref="MdfeDocumento"/>, onde o dado pertence ao fato passado; aqui o
    /// dado pertence ao cadastro vivo.
    /// </summary>
    public class MdfeVeiculo : BaseEntity
    {
        public int IdMdfe { get; set; }
        public MdfeEmissao Mdfe { get; set; }

        /// <summary>Veículo do cadastro (<c>tb_vehicle</c>).</summary>
        public int IdVehicle { get; set; }
        public Vehicle Vehicle { get; set; }

        /// <summary>Tração ou reboque, do ponto de vista DESTA viagem.</summary>
        [Required]
        public MdfePapelVeiculo Tipo { get; set; }
    }
}
