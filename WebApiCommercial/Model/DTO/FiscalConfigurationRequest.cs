
using Microsoft.AspNetCore.Http;
using Model.Enums;
using Model.Registrations;
using System;

namespace WebApiCommercial.Dtos
{
    public class FiscalConfigurationRequest
    {
        // NumeracaoDocumentos simples - campos públicos para binding via form-data/JSON
        public NumeracaoDocumentos? NumeracaoDocumentos { get; set; }

        public CertificadoDigitalRequest? CertificadoDigital { get; set; }

        public Csc? Csc { get; set; }

        public AmbienteEnum Ambiente { get; set; }

        public Emitente? Emitente { get; set; }

        public bool AutorizacaoASO { get; set; }
		public int TenantId { get; set; }

		// Logo da empresa enviada via multipart/form-data
		public IFormFile? LogoFile { get; set; }

		// Sinaliza remoção da logo existente no PUT (sem novo arquivo)
		public bool RemoverLogo { get; set; }

		// Converte DTO para entidade do Model

	}

    public class CertificadoDigitalRequest
    {
        // uso multipart/form-data
        public IFormFile? ArquivoFile { get; set; }

        // alternativa: base64 string enviada no JSON/form
        public string? ArquivoBase64 { get; set; }

        // após salvar, caminho relativo retornado será atribuído aqui antes de mapear para entidade
        public string? Arquivo { get; set; }

        public string? Senha { get; set; }
    }
}