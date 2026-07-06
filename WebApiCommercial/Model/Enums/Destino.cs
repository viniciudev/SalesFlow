using System.Text.Json.Serialization;

namespace Model.Enums
{
    /// <summary>
    /// Destino da operacao fiscal, determinado pela comparacao
    /// entre UF da empresa emitente e UF do cliente destinatario.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Destino
    {
        Interno = 1,
        Interestadual = 2,
        Exterior = 3
    }
}
