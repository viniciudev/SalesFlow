using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MDFe.Classes.Informacoes
{
    [Serializable]
    public class MDFeInfANTT
    {
        /// <summary>
        /// 2 - Registro Nacional de Transportadores Rodoviários de Carga
        /// </summary>
        [XmlElement(ElementName = "RNTRC")]
        public string RNTRC { get; set; }

        /// <summary>
        /// 2 - Dados do CIOT
        /// </summary>
        [XmlElement(ElementName = "infCIOT")]
        public List<infCIOT> InfCIOT { get; set; }

        /// <summary>
        /// 2 - Informações de Vale Pedágio
        /// </summary>
        [XmlElement(ElementName = "valePed")]
        public MDFeValePed ValePed { get; set; }

        /// <summary>
        /// 2 - Grupo de informações dos contratantes do serviço de transporte
        /// </summary>
        [XmlElement(ElementName = "infContratante")]
        public List<MDFeInfContratante> InfContratante { get; set; }

        /// <summary>
        /// 2 - Informações do Pagamento do Frete
        /// </summary>
        [XmlElement(ElementName = "infPag")]
        public List<MDFeInfPag> InfPag { get; set; }
    }
}
