using System;
using System.Xml.Serialization;
using DFe.Classes;

namespace MDFe.Classes.Informacoes
{
    [Serializable]
    public class MDFeDisp
    {
        private decimal _vValePed;

        /// <summary>
        /// 4 - CNPJ da empresa fornecedora do ValePedágio
        /// </summary>
        [XmlElement(ElementName = "CNPJForn")]
        public string CNPJForn { get; set; }

        /// <summary>
        /// 4 - CNPJ do responsável pelo pagamento do Vale-Pedágio
        /// </summary>
        [XmlElement(ElementName = "CNPJPg")]
        public string CNPJPg { get; set; }

        /// <summary>
        /// 4 - CNPJ do responsável pelo pagamento do Vale-Pedágio
        /// </summary>
        public string CPFPg { get; set; }

        /// <summary>
        /// 4 - Número do comprovante de compra 
        /// </summary>
        [XmlElement(ElementName = "nCompra")]
        public string NCompra { get; set; }

        /// <summary>
        /// 4 - Valor do Vale-Pedagio 
        /// </summary>
        [XmlElement(ElementName = "vValePed")]
        public decimal VValePed
        {
            get { return _vValePed.Arredondar(2); }
            set { _vValePed = value.Arredondar(2); }
        }

        /// <summary>
        /// Tipo do Vale Pedágio
        /// </summary>
        [XmlElement(ElementName = "tpValePed")]
        public MDFeTpValePed? TpValePed { get; set; }

        public bool TpValePedSpecified => TpValePed.HasValue;
    }
}