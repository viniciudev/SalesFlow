using System;
using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// As 27 unidades federativas (26 estados + DF), como siglas de 2 letras.
    ///
    /// Existe porque a UF é gravada como <c>string(2)</c> em todo o sistema
    /// (<c>Client.Uf</c>, <c>Provider.uf</c>, <c>DriverLicense.UfEmissaoCnh</c>)
    /// e a única validação vigente é <c>[StringLength(2)]</c> — ou seja, "XX"
    /// passa. O MDF-e precisa de UF de licenciamento real (RV07), então a
    /// validação é feita contra esta lista.
    ///
    /// Não é um enum de propósito: um enum forçaria conversão de/para string em
    /// toda a fronteira com <c>Client.Uf</c> sem ganho, e a comparação aqui é
    /// sempre textual e sensível a maiúsculas (as siglas são normalizadas com
    /// <c>[Uppercase]</c> antes de chegar aqui).
    /// </summary>
    public static class UfList
    {
        public static readonly IReadOnlyList<string> All = new[]
        {
            "AC", "AL", "AM", "AP", "BA", "CE", "DF", "ES", "GO", "MA",
            "MG", "MS", "MT", "PA", "PB", "PE", "PI", "PR", "RJ", "RN",
            "RO", "RR", "RS", "SC", "SE", "SP", "TO"
        };

        private static readonly HashSet<string> _valid =
            new(All, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// true se <paramref name="uf"/> é uma das 27 siglas. Aceita minúsculas
        /// e espaços nas bordas; comparação sem diferenciar caixa.
        /// </summary>
        public static bool IsValid(string? uf)
        {
            if (string.IsNullOrWhiteSpace(uf)) return false;
            return _valid.Contains(uf.Trim());
        }

        /// <summary>Sigla normalizada (maiúsculas, sem espaços) ou null se inválida.</summary>
        public static string? Normalize(string? uf)
        {
            if (string.IsNullOrWhiteSpace(uf)) return null;
            var trimmed = uf.Trim().ToUpperInvariant();
            return _valid.Contains(trimmed) ? trimmed : null;
        }
    }
}
