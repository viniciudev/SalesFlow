using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using MDFe.Classes.Flags;

namespace MDFe.Classes.Informacoes
{
    [Serializable]
    public class MDFeValePed
    {
        /// <summary>
        /// 2 - Informações dos dispositivos do Vale Pedágio
        /// </summary>
        [XmlElement(ElementName = "disp")]
        public List<MDFeDisp> Disp { get; set; }

        /// <summary>
        /// 3 - Categoria de Combinação Veicular
        /// </summary>
        [XmlElement(ElementName = "categCombVeic", IsNullable = true)]
        public MDFeCategCombVeic? CategCombVeic { get; set; }

        public bool CategCombVeicSpecified => CategCombVeic.HasValue; 

        public bool ShouldSerializeCategCombVeic() => CategCombVeicSpecified;

    }
}