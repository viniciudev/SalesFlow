namespace Model.Enums
{
    /// <summary>
    /// Origem de um registro de uso do veículo (<see cref="Model.Registrations.VehicleUsageHistory"/>).
    ///
    /// A RV13 determina que o vínculo motorista × veículo é dinâmico, por
    /// viagem/MDF-e — não há FK fixa. O histórico é o que preserva a
    /// rastreabilidade dessa associação ao longo do tempo, e esta enum diz de
    /// qual documento a associação veio.
    ///
    /// Serialização NUMÉRICA, como em <see cref="WheelType"/>.
    /// </summary>
    public enum VehicleUsageSource
    {
        /// <summary>Uso registrado na emissão de um MDF-e.</summary>
        Mdfe = 1,

        /// <summary>Uso registrado em uma Ordem de Serviço.</summary>
        ServiceOrder = 2
    }
}
