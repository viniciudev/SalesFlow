namespace Model.Enums
{
    /// <summary>
    /// Vínculo do veículo com a empresa. Define se o MDF-e precisa identificar
    /// o proprietário (RV09): apenas <see cref="Proprio"/> dispensa os dados do
    /// proprietário; todos os outros exigem parceiro + CPF/CNPJ + IE + UF + RNTRC.
    ///
    /// Serialização NUMÉRICA, como em <see cref="WheelType"/>.
    /// </summary>
    public enum OwnershipType
    {
        Proprio = 1,
        Terceiro = 2,
        Arrendado = 3,
        Comodato = 4,
        Locado = 5
    }
}
