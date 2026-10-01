using System.ComponentModel;
using System.Xml.Serialization;

namespace MDFe.Classes.Flags
{
    public enum MDFeModal
    {
        [XmlEnum("1")]
        [Description("Rodoviário")]
        Rodoviario = 1,
        [XmlEnum("2")]
        [Description("Aéreo")]
        Aereo = 2,
        [XmlEnum("3")]
        [Description("Aquaviário")]
        Aquaviario = 3,
        [XmlEnum("4")]
        [Description("Ferroviário")]
        Ferroviario = 4
    }
}