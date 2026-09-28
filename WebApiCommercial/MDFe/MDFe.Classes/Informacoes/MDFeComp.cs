using DFe.Classes;
using MDFe.Classes.Flags;
using System;
using System.Xml.Serialization;

namespace MDFe.Classes.Informacoes
{
    [Serializable]
    public class MDFeComp
    {
        /// <summary>
        /// 1 - Tipo do Componente.
        /// </summary>
        [XmlElement(ElementName = "tpComp")]
        public MDFeTpComp TpComp { get; set; }

        [XmlIgnore]
        private decimal _vComp { get; set; }

        /// <summary>
        /// 1 - Valor do Componente.
        /// </summary>
        [XmlElement(ElementName = "vComp")]
        public decimal VComp
        {
            get { return _vComp.Arredondar(2); }
            set { _vComp = value.Arredondar(2); }
        }

        /// <summary>
        /// 1 - Descrição do Componente tipo Outros.
        /// </summary>
        [XmlElement(ElementName = "xComp")]
        public string XComp { get; set; }
    }
}