using System.Text;
using System.Xml.Serialization;
using DFe.Classes.Flags;

namespace MDFe.Classes.Informacoes
{
    public class MdfeInfMDFeSupl
    {
        /// <summary>
        /// 1 -  Texto com o QR-Code para consulta do MDF-e
        /// </summary>
        [XmlElement(ElementName = "qrCodMDFe")]
        public string QrCodMDFe { get; set; }

        public static string GerarQrCode(string chave, TipoAmbiente tipoAmbiente)
        {
            var qrCode = new StringBuilder(@"https://dfe-portal.svrs.rs.gov.br/mdfe/qrCode");
            return qrCode.Append("?").Append("chMDFe=").Append(chave).Append("&").Append("tpAmb=").Append((int)tipoAmbiente).ToString();
        }
    }
}