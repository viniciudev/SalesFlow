namespace Model.Enums
{
    /// <summary>
    /// Tipos especiais de serviço conforme especificação NFS-e 2026 (grupo serv da DPS)
    /// </summary>
    public enum ServiceSpecialType
    {
        /// <summary>
        /// Nenhum tipo especial - serviço padrão
        /// </summary>
        None = 0,

        /// <summary>
        /// Serviço de Obra (infoObra)
        /// </summary>
        Construction = 1,

        /// <summary>
        /// Atividade de Evento (atvEvento)
        /// </summary>
        Event = 2,

        /// <summary>
        /// Comércio Exterior (comExt)
        /// </summary>
        ForeignTrade = 3
    }
}
