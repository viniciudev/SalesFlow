using System.Security.Cryptography.X509Certificates;
using DFe.Utils;
using DFe.Utils.Assinatura;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using NFe.Utils.Annotations;

namespace MDFe.Utils.Configuracoes
{
    public class MDFeConfiguracao : IDisposable, INotifyPropertyChanged
    {
        private static volatile MDFeConfiguracao _instancia;
        private static readonly object SyncRoot = new object();

        private string _caminhoSchemas;
        private bool _deveSalvarXmls;
        private MDFeVersaoWebService _versaoWebService;
        private X509Certificate2 _certificado;

        public MDFeConfiguracao()
        {
            VersaoWebService = new MDFeVersaoWebService();
            ConfiguracaoCertificado = new ConfiguracaoCertificado();
        }

        static MDFeConfiguracao()
        {
        }

        public ConfiguracaoCertificado ConfiguracaoCertificado { get; set; }

        /// <summary>
        ///     Informar se a biblioteca deve salvar o xml de envio e de retorno
        /// </summary>
        public bool IsSalvarXml
        {
            get { return _deveSalvarXmls; }
            set
            {
                if (!value)
                    CaminhoSalvarXml = "";
                _deveSalvarXmls = value;
            }
        }

        /// <summary>
        ///     Diretório onde estão armazenados os schemas para validação
        /// </summary>
        public string CaminhoSchemas
        {
            get { return _caminhoSchemas; }
            set
            {
                if (!string.IsNullOrEmpty(value) && !Directory.Exists(value))
                    throw new Exception("Diretório " + value + " não encontrado!");
                _caminhoSchemas = value;
            }
        }

        /// <summary>
        ///     Diretório onde os xmls de envio/retorno devem ser salvos
        /// </summary>
        public string CaminhoSalvarXml { get; set; }

        public bool IsAdicionaQrCode { get; set; }

        public MDFeVersaoWebService VersaoWebService
        {
            get { return GetMdfeVersaoWebService(); }
            set { _versaoWebService = value; }
        }

        private MDFeVersaoWebService GetMdfeVersaoWebService()
        {
            if (_versaoWebService == null)
                _versaoWebService = new MDFeVersaoWebService();

            return _versaoWebService;
        }
        
        public X509Certificate2 X509Certificate2
        {
            get
            {
                if (_certificado != null && !DeveManterCertificadoEmCache())
                    _certificado.Reset();

                _certificado = ObterCertificado();
                return _certificado;
            }
        }

        public bool NaoSalvarXml()
        {
            return !IsSalvarXml;
        }

        private X509Certificate2 ObterCertificado()
        {
            return CertificadoDigital.ObterCertificado(ConfiguracaoCertificado);
        }

        /// <summary>
        ///     Instância do Singleton de MDFeConfiguracao
        /// </summary>
        public static MDFeConfiguracao Instancia
        {
            get
            {
                if (_instancia != null) return _instancia;
                lock (SyncRoot)
                {
                    if (_instancia != null) return _instancia;
                    _instancia = new MDFeConfiguracao();
                }

                return _instancia;
            }
            set
            {
                lock (SyncRoot)
                {
                    if (value != null)
                        _instancia = value;
                }
            }
        }

        /// <summary>
        ///     Limpa a instancia atual caso exista
        /// </summary>
        public static void LimparInstancia()
        {
            _instancia = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            LimparCertificado();
        }

        ~MDFeConfiguracao()
        {
            LimparCertificado();
        }

        private void LimparCertificado()
        {
            if (DeveManterCertificadoEmCache())
                return;

            _certificado.Reset();
            _certificado = null;
        }

        private bool DeveManterCertificadoEmCache()
        {
            var deveManterCertificadoEmCache = _certificado == null ||
                                               ConfiguracaoCertificado == null ||
                                               ConfiguracaoCertificado.ManterDadosEmCache;

            return deveManterCertificadoEmCache;
        }
    }
}
