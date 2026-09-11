using System;
using System.ComponentModel.DataAnnotations;

namespace Model.Registrations
{
    /// <summary>
    /// Dados de CNH de um parceiro com o perfil <see cref="Enums.PartnerProfile.Motorista"/>.
    ///
    /// Relação 1:1 com <see cref="Client"/> — cada cliente tem no máximo uma CNH.
    /// Guardado em tabela própria (<c>tb_driver_license</c>) para não encher
    /// <c>tb_client</c> de colunas que ficam nulas para quase todos os registros.
    ///
    /// Preparação para MDF-e: o manifesto exige identificação do condutor.
    /// </summary>
    public class DriverLicense : BaseEntity
    {
        public int IdClient { get; set; }
        public Client Client { get; set; }

        /// <summary>Número da CNH — somente dígitos (11).</summary>
        [Required]
        [StringLength(11)]
        public string NumeroCnh { get; set; }

        /// <summary>A, B, C, D, E, AB, AC, AD, AE.</summary>
        [Required]
        [StringLength(2)]
        public string CategoriaCnh { get; set; }

        public DateTime? DataEmissaoCnh { get; set; }

        /// <summary>
        /// Usada para validar CNH vencida (motorista não apto a operar em MDF-e).
        /// </summary>
        [Required]
        public DateTime DataValidadeCnh { get; set; }

        public DateTime? PrimeiraHabilitacao { get; set; }

        [StringLength(2)]
        public string UfEmissaoCnh { get; set; }

        /// <summary>Exerce Atividade Remunerada — exigido no MDF-e.</summary>
        public bool PossuiEar { get; set; }

        [StringLength(500)]
        public string? ObservacoesCnh { get; set; }
    }
}
