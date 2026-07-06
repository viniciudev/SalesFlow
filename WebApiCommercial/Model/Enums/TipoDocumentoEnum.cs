
#nullable enable
using System.Text.Json.Serialization;

namespace Model.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TipoDocumentoEnum
    {
        NFE,
        NFCE
    }
}