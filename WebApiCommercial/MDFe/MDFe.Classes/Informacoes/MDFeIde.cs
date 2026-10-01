using DFe.Classes.Entidades;
using DFe.Classes.Extensoes;
using DFe.Classes.Flags;
using DFe.Utils;
using MDFe.Classes.Flags;
using MDFe.Utils.Configuracoes;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using VersaoServico = MDFe.Utils.Flags.VersaoServico;

namespace MDFe.Classes.Informacoes
{
    [Serializable]
    public class MDFeIde
    {
        private MDFeIde()
        {
            InfMunCarrega = new List<MDFeInfMunCarrega>();
            InfPercurso = new List<MDFeInfPercurso>();
        }

        public MDFeIde(VersaoServico? versaoServico = null) : this()
        {
            _versaoServico = versaoServico ?? MDFeConfiguracao.Instancia.VersaoWebService.VersaoLayout;
        }

        [XmlIgnore]
        private VersaoServico? _versaoServico;

        /// <summary>
        /// Versão do leiaute usada para escrever <c>dhEmi</c> e <c>dhIniViagem</c>.
        ///
        /// <b>Patch local sobre a biblioteca do DFe.NET.</b> O <c>XmlSerializer</c>
        /// cria este objeto pelo construtor privado, que não tem como receber a
        /// versão — então todo <see cref="MDFeIde"/> que veio de
        /// <c>XmlStringParaClasse</c> fica com <c>_versaoServico == null</c> e cai no
        /// fallback para <c>MDFeConfiguracao.Instancia</c>. Num serviço web
        /// multi-empresa esse singleton nunca é configurado (é compartilhado entre
        /// requisições), o valor fica <c>default(VersaoServico) == 0</c>, e os proxies
        /// abaixo lançam "Versão Inválida para MDF-e" no meio da serialização — que é
        /// onde a transmissão monta o envelope SOAP.
        ///
        /// Quem desserializa informa aqui a versão do próprio documento
        /// (<c>infMDFe/@versao</c>), que é a fonte da verdade: é ela que diz como os
        /// campos de data foram escritos e, portanto, como devem ser reescritos.
        /// </summary>
        [XmlIgnore]
        public VersaoServico VersaoLayout
        {
            get { return _versaoServico ?? MDFeConfiguracao.Instancia.VersaoWebService.VersaoLayout; }
            set { _versaoServico = value; }
        }

        /// <summary>
        /// 2 - Código da UF do emitente do MDF-e. 
        /// </summary>
        [XmlElement(ElementName = "cUF")]
        public Estado CUF { get; set; }

        /// <summary>
        /// 2 - Tipo do Ambiente 
        /// </summary>
        [XmlElement(ElementName = "tpAmb")]
        public TipoAmbiente TpAmb { get; set; }

        /// <summary>
        /// 2 - Tipo do Emitente 
        /// </summary>
        [XmlElement(ElementName = "tpEmit")]
        public MDFeTipoEmitente TpEmit { get; set; }

        /// <summary>
        /// MDF-e 3.0
        /// Tipo do Transportador
        /// Opcional
        /// </summary>
        [XmlElement(ElementName = "tpTransp")]
        public MDFeTpTransp? TpTransp { get; set; }

        public bool TpTranspSpecified { get { return TpTransp.HasValue; } }

        /// <summary>
        /// 2 - Modelo do Manifesto Eletrônico
        /// </summary>
        [XmlElement(ElementName = "mod")]
        public ModeloDocumento Mod { get; set; }

        /// <summary>
        /// 2- Série do Manifesto
        /// </summary>
        [XmlElement(ElementName = "serie")]
        public short Serie { get; set; }

        /// <summary>
        /// 2- Número do Manifesto 
        /// </summary>
        [XmlElement(ElementName = "nMDF")]
        public long NMDF { get; set; }

        /// <summary>
        /// 2 - Código numérico que compõe a Chave de Acesso. 
        /// </summary>
        [XmlIgnore]
        public int CMDF { get; set; }

        /// <summary>
        /// Proxy para cMDF
        /// </summary>
        [XmlElement(ElementName = "cMDF")]
        public string ProxyCMDF
        {
            get { return CMDF.ToString("00000000"); }
            set { CMDF = int.Parse(value); }
        }

        /// <summary>
        /// 2 - Digito verificador da chave de acesso do Manifesto
        /// </summary>
        [XmlElement(ElementName = "cDV")]
        public byte CDV { get; set; }

        /// <summary>
        /// 2 - Modalidade de transporte 
        /// </summary>
        [XmlElement(ElementName = "modal")]
        public MDFeModal Modal { get; set; }

        /// <summary>
        /// 2 - Data e hora de emissão do Manifesto 
        /// </summary>
        [XmlIgnore]
        public DateTime DhEmi { get; set; }

        /// <summary>
        /// Proxy para dhEmi
        /// </summary>
        [XmlElement(ElementName = "dhEmi")]
        public string ProxyDhEmi
        {
            get
            {
                switch (VersaoLayout)
                {
                    case VersaoServico.Versao100:
                        return DhEmi.ParaDataHoraStringSemUtc();
                    case VersaoServico.Versao300:
                        return DhEmi.ParaDataHoraStringUtc();
                    default:
                        throw new InvalidOperationException("Versão Inválida para MDF-e");
                }

            }
            set { DhEmi = DateTime.Parse(value); }
        }

        /// <summary>
        /// 2 - Forma de emissão do Manifesto (Normal ou Contingência)
        /// </summary>
        [XmlElement(ElementName = "tpEmis")]
        public MDFeTipoEmissao TpEmis { get; set; }

        /// <summary>
        /// 2 - Identificação do processo de emissão do Manifesto
        /// </summary>
        [XmlElement(ElementName = "procEmi")]
        public MDFeIdentificacaoProcessoEmissao ProcEmi { get; set; }

        /// <summary>
        /// 2 - Versão do processo de emissão 
        /// </summary>
        [XmlElement(ElementName = "verProc")]
        public string VerProc { get; set; }

        /// <summary>
        /// 2 - Sigla da UF do Carregamento 
        /// </summary>
        [XmlIgnore]
        public Estado UFIni { get; set; }

        /// <summary>
        /// Proxy para UFIni
        /// </summary>
        [XmlElement(ElementName = "UFIni")]
        public string ProxyUFIni
        {
            get { return UFIni.GetSiglaUfString(); }
            set { UFIni = UFIni.SiglaParaEstado(value); }
        }

        /// <summary>
        /// 2 - Sigla da UF do Descarregamento
        /// </summary>
        [XmlIgnore]
        public Estado UFFim { get; set; }

        /// <summary>
        /// Proxy para UFFim
        /// </summary>
        [XmlElement(ElementName = "UFFim")]
        public string ProxyUFFim
        {
            get { return UFFim.GetSiglaUfString(); }
            set { UFFim = UFFim.SiglaParaEstado(value); }
        }

        /// <summary>
        /// 2 - Informações dos Municípios de Carregamento
        /// </summary>
        [XmlElement(ElementName = "infMunCarrega")]
        public List<MDFeInfMunCarrega> InfMunCarrega { get; set; }

        /// <summary>
        /// 2 - Informações do Percurso do MDF-e 
        /// </summary>
        [XmlElement(ElementName = "infPercurso")]
        public List<MDFeInfPercurso> InfPercurso { get; set; }

        /// <summary>
        /// 2 - Data e hora previstos de inicio da viagem
        /// </summary>
        [XmlIgnore]
        public DateTime? DhIniViagem { get; set; }

        /// <summary>
        /// Proxy para dhIniViagem
        /// </summary>
        [XmlElement(ElementName = "dhIniViagem")]
        public string ProxyDhIniViagem 
        {
            get
            {
                switch (VersaoLayout)
                {
                    case VersaoServico.Versao100:
                        return DhIniViagem.ParaDataHoraStringSemUtc();
                    case VersaoServico.Versao300:
                        return DhIniViagem.ParaDataHoraStringUtc();
                    default:
                        throw new InvalidOperationException("Versão Inválida para MDF-e");
                }
            }
            set
            {
                if (value is null)
                    DhIniViagem = null;
                else
                    DhIniViagem = DateTime.Parse(value);
            }
        }

        /// <summary>
        /// Indicador de participação do Canal Verde.
        /// </summary>
        [XmlElement(ElementName = "indCanalVerde")]
        public string IndCanalVerde { get; set; }

        /// <summary>
        /// Indicador de MDF-e com inclusão da Carga posterior
        /// a emissão por evento de inclusão de DF-e.
        /// </summary>
        [XmlElement(ElementName = "indCarregaPosterior")]
        public string IndCarregaPosterior { get; set; }
    }
}
