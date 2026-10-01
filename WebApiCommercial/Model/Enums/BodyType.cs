namespace Model.Enums
{
    /// <summary>
    /// Tipo de carroceria do veículo — domínio fechado do MOC (MDF-e, campo
    /// <c>tipoCarroceria</c>).
    ///
    /// Os valores numéricos são o contrato com o MOC e com o front-end: NÃO
    /// reordene nem renumere. Serialização NUMÉRICA, como em
    /// <see cref="WheelType"/>.
    /// </summary>
    public enum BodyType
    {
        /// <summary>00 - Não aplicável</summary>
        NaoAplicavel = 0,

        /// <summary>01 - Aberta</summary>
        Aberta = 1,

        /// <summary>02 - Fechada / Baú</summary>
        FechadaBau = 2,

        /// <summary>
        /// 03 - Granelera. É a carroceria prioritária da operação (cimento):
        /// é o caso de uso que motiva este cadastro.
        /// </summary>
        Granelera = 3,

        /// <summary>04 - Porta Container</summary>
        PortaContainer = 4,

        /// <summary>05 - Sider</summary>
        Sider = 5
    }
}
