using System;
using System.Xml.Serialization;

namespace MDFe.Classes.Informacoes
{
    [Serializable]
    public class MDFeInfLotacao
    {
        public MDFeInfLotacao()
        {
            InfLocalCarrega = new MDFeInfLocalCarrega();
            InfLocalDescarrega = new MDFeInfLocalDescarrega();
        }

        /// <summary>
        /// 1 - Informações da localização do
        /// carregamento do MDF-e de carga lotação
        /// </summary>
        [XmlElement(ElementName = "infLocalCarrega")]
        public MDFeInfLocalCarrega InfLocalCarrega { get; set; }

        /// <summary>
        /// 1 - Informações da localização do
        /// descarregamento do MDF-e de carga
        /// lotação
        /// </summary>
        [XmlElement(ElementName = "infLocalDescarrega")]
        public MDFeInfLocalDescarrega InfLocalDescarrega { get; set; }
    }
}