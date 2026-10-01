namespace Model.Enums
{
    /// <summary>
    /// Categoria do proprietário terceiro, exigida pelo MOC quando o veículo
    /// não é próprio. Diferencia o transportador autônomo de carga (TAC) que
    /// atua como agregado de uma empresa do que atua de forma independente.
    ///
    /// Serialização NUMÉRICA, como em <see cref="WheelType"/>.
    /// </summary>
    public enum ThirdPartyCategory
    {
        /// <summary>TAC Agregado — autônomo vinculado à empresa.</summary>
        TacAgregado = 0,

        /// <summary>TAC Independente.</summary>
        TacIndependente = 1,

        /// <summary>Outros (ex.: transportadora, locadora).</summary>
        Outros = 2
    }
}
