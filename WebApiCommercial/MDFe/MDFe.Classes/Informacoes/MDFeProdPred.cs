using System;
using System.Xml.Serialization;
using MDFe.Classes.Flags;

namespace MDFe.Classes.Informacoes
{
    [Serializable]
    public class MDFeProdPred
    {
        /// <summary>
        /// 1 - Tipo da Carga.
        /// Conforme Rosulação ANTT
        /// </summary>
        [XmlElement(ElementName = "tpCarga")]
        public MDFeTpCarga TpCarga { get; set; }

        /// <summary>
        /// 1 - Descrição do produto predominante.
        /// </summary>
        [XmlElement(ElementName = "xProd")]
        public string XProd { get; set; }

        /// <summary>
        /// 1- GTIN (Global Trade Item Number) do produto, antigo código EAN ou código de barras.
        /// </summary>
        [XmlElement(ElementName = "cEAN")]
        public string CEan { get; set; }

        /// <summary>
        /// 1 - Código NCM
        /// </summary>
        [XmlElement(ElementName = "NCM")]
        public string Ncm { get; set; }

        /// <summary>
        /// 1 - Informações da carga lotação. Informar somente quando MDF-e for de carga lotação
        /// </summary>
        [XmlElement(ElementName = "infLotacao")]
        public MDFeInfLotacao InfLotacao { get; set; }
    }
}