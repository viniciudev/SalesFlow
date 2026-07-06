
#nullable enable
using System.Text.Json.Serialization;

namespace Model.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FinalidadeEnum
    {
        NORMAL,
        DEVOLUCAO,
        AJUSTE
    }
}