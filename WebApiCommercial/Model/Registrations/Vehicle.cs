using Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.Registrations
{
    /// <summary>
    /// Veículo de tração ou reboque, cadastrado para a futura emissão de MDF-e
    /// (MOC/SEFAZ).
    ///
    /// É o par de <see cref="DriverLicense"/>: aquele cobre o condutor exigido
    /// pelo manifesto, este cobre o veículo (<c>veicTracao</c> /
    /// <c>veicReboque</c>). Os dois campos que a SEFAZ rejeita com mais
    /// frequência quando faltam — <c>tipoRodado</c> e <c>tipoCarroceria</c> —
    /// são obrigatórios já aqui, e a tara é validada como positiva.
    ///
    /// Exclusão é SEMPRE lógica (RV11): nunca há DELETE físico. Como o projeto
    /// não tem filtro global de soft delete (não há <c>HasQueryFilter</c>
    /// nem <c>ISoftDeletable</c>), todo filtro por <see cref="IsActive"/> é
    /// explícito no <c>VehicleRepository</c> — não confie em filtro automático.
    ///
    /// RV13: NÃO existe FK fixa motorista × veículo. A vinculação acontece por
    /// viagem/MDF-e e é preservada em <see cref="VehicleUsageHistory"/>.
    /// </summary>
    public class Vehicle : BaseEntity
    {
        /// <summary>Empresa dona do cadastro (multi-tenant).</summary>
        public int IdCompany { get; set; }
        public Company Company { get; set; }

        /// <summary>Identificador interno de frota, livre. Opcional.</summary>
        [StringLength(30)]
        public string? InternalCode { get; set; }

        /// <summary>
        /// Placa Mercosul (ABC1D23) ou antiga (ABC1234), gravada SEM separador e
        /// em maiúsculas. Normalizada por <c>[Uppercase]</c> em
        /// <c>ContextBase.SaveChangesAsync</c>.
        ///
        /// Única entre veículos ATIVOS da mesma empresa (RV02) — índice único
        /// parcial em <c>tb_vehicle</c>. Desativar um veículo libera a placa.
        /// </summary>
        [Required]
        [StringLength(8)]
        [Uppercase]
        public string LicensePlate { get; set; }

        /// <summary>RENAVAM — somente dígitos, 11 posições (RV03). Opcional.</summary>
        [StringLength(11)]
        public string? Renavam { get; set; }

        /// <summary>UF de licenciamento (RV07) — uma das 27 siglas de <see cref="UfList"/>.</summary>
        [Required]
        [StringLength(2)]
        [Uppercase]
        public string LicensingState { get; set; }

        /// <summary>Tração ou reboque.</summary>
        [Required]
        public VehicleType VehicleType { get; set; }

        /// <summary>Tipo de rodado — domínio fechado do MOC (RV05).</summary>
        [Required]
        public WheelType WheelType { get; set; }

        /// <summary>Tipo de carroceria — domínio fechado do MOC (RV06).</summary>
        [Required]
        public BodyType BodyType { get; set; }

        /// <summary>
        /// Tara em KG. Obrigatória e maior que zero (RV04): é campo do XML do
        /// MDF-e e a SEFAZ rejeita tara zerada.
        /// </summary>
        [Required]
        public int Tare { get; set; }

        /// <summary>Capacidade de carga em KG. Opcional (o MOC só limita o máximo).</summary>
        public int? CapacityKg { get; set; }

        /// <summary>Capacidade de carga em M³. Opcional.</summary>
        public decimal? CapacityM3 { get; set; }

        /// <summary>
        /// Vínculo com a empresa (RV08). Se diferente de
        /// <see cref="OwnershipType.Proprio"/>, os dados do proprietário passam a
        /// ser exigidos (RV09) — ver <see cref="ClientId"/>.
        /// </summary>
        [Required]
        public OwnershipType OwnershipType { get; set; }

        /// <summary>
        /// Categoria do proprietário terceiro (TAC agregado/independente/outros).
        /// Exigida quando <see cref="OwnershipType"/> != Próprio; nula para
        /// veículo próprio.
        /// </summary>
        public ThirdPartyCategory? ThirdPartyCategory { get; set; }

        /// <summary>
        /// Proprietário/locador do veículo quando não é próprio (RV09/RV10).
        /// Aponta para o PARCEIRO (<see cref="Client"/> — tabela <c>tb_client</c>),
        /// que já concentra CPF/CNPJ (<c>Document</c>), <c>Ie</c>, <c>Uf</c> e,
        /// a partir deste módulo, <c>Rntrc</c>.
        ///
        /// Nulo quando <see cref="OwnershipType"/> == Próprio.
        /// </summary>
        public int? ClientId { get; set; }
        public Client? Client { get; set; }

        /// <summary>
        /// false = excluído logicamente (RV11). Veículos inativos continuam na
        /// listagem, mas ficam fora de <c>available-for-mdfe</c> (RV12).
        /// </summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>Histórico de associação a MDF-e/OS (RV14).</summary>
        public ICollection<VehicleUsageHistory> UsageHistory { get; set; }
            = new List<VehicleUsageHistory>();
    }
}
