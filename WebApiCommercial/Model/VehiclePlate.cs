using System.Linq;
using System.Text.RegularExpressions;

namespace Model
{
    /// <summary>
    /// Normalização e validação de placa de veículo (RV01).
    ///
    /// Aceita os dois formatos em circulação no Brasil:
    /// <list type="bullet">
    ///   <item>Mercosul: 3 letras + 1 dígito + 1 letra/dígito + 2 dígitos (ABC1D23)</item>
    ///   <item>Antigo: 3 letras + 4 dígitos (ABC1234)</item>
    /// </list>
    ///
    /// As duas formas caem na MESMA expressão: a 5ª posição é
    /// <c>[A-Za-z0-9]</c>, que cobre tanto a letra do Mercosul quanto o dígito
    /// do formato antigo. Um único padrão evita a classe de bug em que a placa
    /// antiga é rejeitada por um regex escrito só para Mercosul.
    ///
    /// Fica fora de <c>Vehicle</c> de propósito: é regra de formato pura, sem
    /// estado e sem dependência do EF — o mesmo código serve para o serviço, e
    /// mais tarde para o gerador do XML do MDF-e.
    /// </summary>
    public static class VehiclePlate
    {
        /// <summary>Placa Mercosul (ABC1D23) ou antiga (ABC1234).</summary>
        public static readonly Regex Pattern =
            new(@"^[A-Za-z]{3}[0-9][A-Za-z0-9][0-9]{2}$", RegexOptions.Compiled);

        /// <summary>
        /// Tira separadores (hífen, espaço, ponto), remove acentuação de
        /// conveniência e devolve em MAIÚSCULAS. Devolve string vazia se a
        /// entrada for nula — para que o chamador trate "sem placa" e "placa
        /// inválida" pelo mesmo caminho (RV01).
        /// </summary>
        public static string Normalize(string? plate)
        {
            if (string.IsNullOrWhiteSpace(plate)) return string.Empty;

            var cleaned = new string(plate
                .Where(char.IsLetterOrDigit)
                .ToArray());

            return cleaned.ToUpperInvariant();
        }

        /// <summary>
        /// true se a placa (já normalizada ou não) está em um dos dois formatos.
        /// Normaliza internamente, então aceita "abc-1234".
        /// </summary>
        public static bool IsValid(string? plate)
        {
            var normalized = Normalize(plate);
            return normalized.Length > 0 && Pattern.IsMatch(normalized);
        }

        /// <summary>Versão numérica do RENAVAM do MDF-e (11 dígitos, RV03).</summary>
        public static string? NormalizeRenavam(string? renavam)
        {
            if (string.IsNullOrWhiteSpace(renavam)) return null;

            var digits = new string(renavam.Where(char.IsDigit).ToArray());
            return digits.Length == 0 ? null : digits;
        }

        /// <summary>true se o RENAVAM informado tem exatamente 11 dígitos.</summary>
        public static bool IsValidRenavam(string? renavam)
        {
            if (string.IsNullOrWhiteSpace(renavam)) return true; // opcional (RV03)

            var digits = NormalizeRenavam(renavam);
            return digits != null && digits.Length == 11;
        }
    }
}
