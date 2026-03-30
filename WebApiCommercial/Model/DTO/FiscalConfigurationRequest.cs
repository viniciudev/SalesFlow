
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

        // Converte DTO para entidade do Model
        public Model.Registrations.FiscalConfiguration ToEntity()
        {
            return new Model.Registrations.FiscalConfiguration
            {
                // Id será atribuído pelo controller / service quando necessário
                NumeracaoDocumentos = NumeracaoDocumentos ?? new NumeracaoDocumentos(),
                CertificadoDigital = new CertificadoDigital
                {
                    Arquivo = CertificadoDigital?.Arquivo,
                    Senha = CertificadoDigital?.Senha
                },
                Csc = Csc ?? new Csc(),
                Ambiente = Ambiente,
                Emitente = Emitente ?? new Emitente(),
                AutorizacaoASO = AutorizacaoASO
            };
        }
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