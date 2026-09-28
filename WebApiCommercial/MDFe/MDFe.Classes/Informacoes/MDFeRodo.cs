using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using MDFe.Classes.Contratos;

namespace MDFe.Classes.Informacoes
{
    [Serializable]
    public class MDFeRodo : MDFeModalContainer
    {
        [XmlElement(ElementName = "infANTT")]
        public MDFeInfANTT InfANTT { get; set; }

        /// <summary>
        /// 1 - Registro Nacional de Transportadores Rodoviários de Carga
        /// </summary>
        [XmlElement(ElementName = "RNTRC")]
        public string RNTRC { get; set; }

        /// <summary>
        /// 1 - Código Identificador da Operação de Transporte
        /// </summary>
        [XmlElement(ElementName = "CIOT")]
        public string CIOT { get; set; }

        /// <summary>
        /// 1 - Dados do Veículo com a Tração
        /// </summary>
        [XmlElement(ElementName = "veicTracao")]
        public MDFeVeicTracao VeicTracao { get; set; }

        /// <summary>
        /// 1 - Dados dos reboques
        /// </summary>
        [XmlElement(ElementName = "veicReboque")]
        public List<MDFeVeicReboque> VeicReboque { get; set; }

        /// <summary>
        /// 1 - Informações de Vale Pedágio
        /// </summary>
        [XmlElement(ElementName = "valePed")]
        public MDFeValePed ValePed { get; set; }

        /// <summary>
        /// 1 - Código de Agendamento no porto 
        /// </summary>
        [XmlElement(ElementName = "codAgPorto")]
        public string CodAgPorto { get; set; }

        /// <summary>
        /// 1 - Lacres 
        /// </summary>
        [XmlElement(ElementName = "lacRodo")]
        public List<MDFeLacre> LacRodo { get; set; }
    }
}