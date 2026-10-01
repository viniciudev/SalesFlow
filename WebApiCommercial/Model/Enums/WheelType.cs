namespace Model.Enums
{
    /// <summary>
    /// Tipo de rodado do veículo — domínio fechado do MOC (MDF-e, campo
    /// <c>tipoRodado</c>).
    ///
    /// Os valores numéricos são o contrato com o MOC e com o front-end: NÃO
    /// reordene nem renumere. A serialização da API é NUMÉRICA (Newtonsoft sem
    /// <c>JsonStringEnumConverter</c> — mesmo padrão documentado em
    /// <see cref="PartnerProfile"/>), então o front-end espelha estes inteiros
    /// em <c>frontEnd/studio/src/lib/types/vehicle.ts</c>.
    /// </summary>
    public enum WheelType
    {
        /// <summary>01 - Truck</summary>
        Truck = 1,

        /// <summary>02 - Toco</summary>
        Toco = 2,

        /// <summary>03 - Cavalo Mecânico</summary>
        CavaloMecanico = 3,

        /// <summary>04 - VAN</summary>
        Van = 4,

        /// <summary>05 - Utilitário</summary>
        Utilitario = 5,

        /// <summary>06 - Outros</summary>
        Outros = 6
    }
}
