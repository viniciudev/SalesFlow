using DFe.Classes;
using MDFe.Classes.Flags;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MDFe.Classes.Informacoes
{
    [Serializable]
    public class MDFeInfPag
    {
        public MDFeInfPag()
        {
            InfPrazo = new List<MDFeInfPrazo>();
        }

        /// <summary>
        /// 3 - Nome do responsável pelo pagamento.
        /// </summary>
        [XmlElement(ElementName = "xNome")]
        public string XNome { get; set; }

        /// <summary>
        /// 3 - Número do CPF do responsável pelo pagamento.
        /// </summary>
        [XmlElement(ElementName = "CPF")]
        public string CPF { get; set; }

        /// <summary>
        /// 3 - Número do CNPJ do responsável pelo pagamento.
        /// </summary>
        [XmlElement(ElementName = "CNPJ")]
        public string CNPJ { get; set; }

        /// <summary>
        /// 3 - Identificador do responsável pelo pagamento 
        /// em caso de ser estrangeiro.
        /// </summary>
        [XmlElement(ElementName = "idEstrangeiro")]
        public string IdEstrangeiro { get; set; }

        /// <summary>
        /// 3 - Componentes do pagamento do frete.
        /// </summary>
        [XmlElement(ElementName = "Comp")]
        public List<MDFeComp> Comp { get; set; }

        [XmlIgnore]
        private decimal _vContrato { get; set; }

        /// <summary>
        /// 3 - Valor total do Contrato.
        /// </summary>
        [XmlElement("vContrato")]
        public decimal VContratoProxy
        {
            get { return _vContrato.Arredondar(2); }
            set { _vContrato = value.Arredondar(2); }
        }

        /// <summary>
        /// 3 - Indicador da forma de pagamento.
        /// </summary>
        [XmlElement(ElementName = "indPag")]
        public MDFeIndPag IndPag { get; set; }

        /// <summary>
        /// 3 - Informações do pagamento a prazo. Informar somente se indPag for à Prazo.
        /// </summary>
        [XmlElement(ElementName = "infPrazo")]
        public List<MDFeInfPrazo> InfPrazo { get; set; }

        /// <summary>
        /// 3 - Informações Bancárias.
        /// </summary>
        [XmlElement(ElementName = "infBanc")]
        public MDFeInfBanc InfBanc { get; set; }

        /// <summary>
        /// 3 - Indicador de operação de transporte de 
        /// alto desempenho
        /// </summary>
        [XmlElement(ElementName = "indAltoDesemp")]
        public MDFeIndAltoDesemp IndAltoDesemp { get; set; }

        public bool ShouldSerializeIndAltoDesemp()
        {
            return IndAltoDesemp == MDFeIndAltoDesemp.AltoDesempenho;
        }
    }
}