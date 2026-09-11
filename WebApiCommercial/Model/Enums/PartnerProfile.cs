using System;

namespace Model.Enums
{
    /// <summary>
    /// Classificações que um parceiro pode acumular simultaneamente.
    /// Um mesmo registro pode ser, por exemplo, Transportadora E Motorista.
    ///
    /// É um enum [Flags] persistido como <c>integer</c> (bitmask) em
    /// <c>tb_client.Profiles</c>. A serialização é NUMÉRICA — sem
    /// <c>JsonStringEnumConverter</c>, seguindo o padrão vigente da API
    /// (Newtonsoft sem conversor de enum, ver Startup.cs).
    ///
    /// ATENÇÃO: os pesos são contrato com o front-end. Não reordene nem
    /// renumere valores já usados — apenas acrescente potências de 2.
    /// Espelhado em <c>frontEnd/studio/src/lib/types/client.ts</c>
    /// (PARTNER_PROFILE_BITS).
    /// </summary>
    [Flags]
    public enum PartnerProfile
    {
        None = 0,
        Cliente = 1,
        Fornecedor = 2,
        Transportadora = 4,
        Motorista = 8,
        Outros = 16,
    }
}
