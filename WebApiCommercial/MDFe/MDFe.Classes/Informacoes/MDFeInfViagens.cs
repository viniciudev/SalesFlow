using System.Xml.Serialization;
using System;

namespace MDFe.Classes.Informacoes
{
    [Serializable]
    public class MDFeInfViagens
    {
        [XmlIgnore]
        private int _qtdViagens { get; set; }

        /// <summary>
        /// 2 - Proxy para quantidade total de viagens realizadas com o pagamento do frete.
        /// </summary>
        [XmlElement("qtdViagens")]
        public string QtdViagensProxy
        {
            get { return _qtdViagens.ToString("D5"); }
            set { _qtdViagens = int.Parse(value); }
        }

        [XmlIgnore]
        private int _nroViagem { get; set; }

        /// <summary>
        /// 2 - Proxy para número de referência da viagem do MDF-e referenciado.
        /// </summary>
        [XmlElement("nroViagem")]
        public string NroViagemProxy
        {
            get { return _nroViagem.ToString("D5"); }
            set { _nroViagem = int.Parse(value); }
        }
    }
}