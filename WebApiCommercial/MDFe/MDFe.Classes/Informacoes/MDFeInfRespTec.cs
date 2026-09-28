using System.Xml.Serialization;

namespace MDFe.Classes.Informacoes
{
    public class MDFeInfRespTec
    {
        /// <summary>
        /// 2 - CNPJ da pessoa jurídica responsável técnica pelo sistema
        /// utilizado na emissão do documento fiscal eletrônico
        /// </summary>
        [XmlElement(ElementName = "CNPJ")]
        public string CNPJ { get; set; }

        /// <summary>
        /// 2 - Nome da pessoa a ser contatada 
        /// </summary>
        [XmlElement(ElementName = "xContato")]
        public string XContato { get; set; }

        /// <summary>
        /// 2 - E-mail da pessoa jurídica a ser contatada
        /// </summary>
        [XmlElement(ElementName = "email")]
        public string Email { get; set; }

        /// <summary>
        /// 2 - Telefone da pessoa jurídica a ser contatada
        /// </summary>
        [XmlElement(ElementName = "fone")]
        public string Fone { get; set; }

        [XmlIgnore]
        private int? IdCSRT { get; set; }

        public bool IdCSRTSpecified
        {
            get { return IdCSRT.HasValue; }
        }

        /// <summary>
        /// 2 - Identificador do código de segurança do responsável técnico
        /// </summary>
        [XmlElement(ElementName = "idCSRT")]
        public string ProxyIdCSRT
        {
            get { return IdCSRT != null ? IdCSRT.Value.ToString("D3") : null; }
            set { IdCSRT = int.Parse(value); }
        }

        /// <summary>
        /// 2 - Hash do token do código de segurança do responsável técnico
        /// </summary>
        [XmlElement(ElementName = "hashCSRT")]
        public string HashCSRT { get; set; }
    }
}