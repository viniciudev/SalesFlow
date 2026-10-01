using System;
using System.Xml.Serialization;
using DFe.Classes;
using DFe.Utils;

namespace MDFe.Classes.Informacoes
{
    [Serializable]
    public class MDFeInfPrazo
    {
        [XmlIgnore]
        private short _nParcela { get; set; }

        /// <summary>
        /// 1 - Número da parcela.
        /// </summary>
        [XmlElement("nParcela")]
        public string NParcelaProxy
        {
            get { return _nParcela.ToString("D3"); }
            set { _nParcela = short.Parse(value); }
        }

        [XmlIgnore]
        private DateTime _dVenc { get; set; }

        /// <summary>
        /// Proxy para Data de Vencimento da parcela.
        /// </summary>
        [XmlElement("dVenc")]
        public string DVencProxy
        {
            get { return _dVenc.ParaDataString(); }
            set { _dVenc = DateTime.Parse(value); }
        }

        [XmlIgnore]
        private decimal _vParcela { get; set; }

        /// <summary>
        /// 1 - Valor da parcela.
        /// </summary>
        [XmlElement("vParcela")]
        public decimal VParcelaProxy
        {
            get { return _vParcela.Arredondar(2); }
            set { _vParcela = value.Arredondar(2); }
        }
    }
}