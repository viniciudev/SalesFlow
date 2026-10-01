using System;
using System.Xml.Serialization;

namespace MDFe.Classes.Informacoes
{
    [Serializable]
    public class MDFeInfLocalCarrega
    {
        /// <summary>
        /// 1 - Cep onde foi carregado o MDF-e.
        /// </summary>
        [XmlElement(ElementName = "CEP")]
        public string CEP { get; set; }

        [XmlIgnore]
        private decimal? _latitude { get; set; }

        /// <summary>
        /// 1- Latitude do ponto geográfico onde foi carregado o MDF-e.
        /// </summary>
        [XmlElement("latitude")]
        public string LatitudeProxy
        {
            get
            {
                if (_latitude == null) return null;
                return _latitude.ToString();
            }
            set
            {
                if (value == null)
                {
                    _latitude = null;
                    return;
                }
                _latitude = decimal.Parse(value);
            }
        }

        [XmlIgnore]
        private decimal? _longitude { get; set; }

        /// <summary>
        /// 1 - Longitude do ponto geográfico onde foi carregado o MDF-e.
        /// </summary>
        [XmlElement("longitude")]
        public string LongitudeProxy
        {
            get
            {
                if (_longitude == null) return null;
                return _longitude.ToString();
            }
            set
            {
                if (value == null)
                {
                    _longitude = null;
                    return;
                }
                _longitude = decimal.Parse(value);
            }
        }
    }
}