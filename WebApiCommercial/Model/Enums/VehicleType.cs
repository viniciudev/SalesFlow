namespace Model.Enums
{
    /// <summary>
    /// Papel do veículo na composição de transporte do MDF-e: o que traciona
    /// (<c>veicTracao</c>) ou o que é rebocado (<c>veicReboque</c>, 0 a 5 por
    /// manifesto, conforme o XSD <c>mdfeModalRodoviario_v3.00.xsd</c>).
    ///
    /// Serialização NUMÉRICA, como em <see cref="WheelType"/>.
    /// </summary>
    public enum VehicleType
    {
        /// <summary>Veículo de tração (cavalo mecânico, truck, toco, VAN...).</summary>
        Traction = 1,

        /// <summary>Reboque / semirreboque.</summary>
        Trailer = 2
    }
}
