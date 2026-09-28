using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MDFe.Classes.Informacoes.Evento.CorpoEvento
{
    [Serializable]
    public class MDFeEvPagtoOperMDFe : MDFeEventoContainer
    {
        public MDFeEvPagtoOperMDFe()
        {
            DescEvento = "Pagamento Operacao MDF-e";
        }

        /// <summary>
        /// 1 - Descrição do evento
        /// </summary>
        [XmlElement("descEvento")]
        public string DescEvento { get; set; }

        /// <summary>
        /// 1 - Número do protocolo de autorização do MDF-e
        /// </summary>
        [XmlElement("nProt")]
        public string NProt { get; set; }

        /// <summary>
        /// 1 - Informações do total de viagens acobertadas pelo Evento “pagamento do frete” 
        /// </summary>
        [XmlElement("infViagens")]
        public MDFeInfViagens InfViagens { get; set; }

        /// <summary>
        /// 1 - Grupo de Informações dos pgto do MDF-e
        /// </summary>
        [XmlElement(ElementName = "infPag")]
        public List<MDFeInfPag> InfPag { get; set; }
    }
}