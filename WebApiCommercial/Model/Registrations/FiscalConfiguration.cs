#nullable enable
using System;

namespace Model.Registrations
{
    public class FiscalConfiguration : BaseEntity
    {
        // NumeracaoDocumentos
        public NumeracaoDocumentos NumeracaoDocumentos { get; set; } = new();

        // CertificadoDigital
        public CertificadoDigital CertificadoDigital { get; set; } = new();

        // CSC
        public Csc Csc { get; set; } = new();

        // Ambiente (string stored as enum in EF mapping)
        public Model.Enums.AmbienteEnum Ambiente { get; set; }

        // Emitente
        public Emitente Emitente { get; set; } = new();

        // Autorizacao ASO
        public bool AutorizacaoASO { get; set; }
    }

    public class NumeracaoDocumentos
    {
        public NumeracaoItem Nfe { get; set; } = new();
        public NumeracaoItem Nfce { get; set; } = new();
    }

    public class NumeracaoItem
    {
        public string? Serie { get; set; }
        public long NumeroInicial { get; set; }
    }

    public class CertificadoDigital
    {
        // pode armazenar caminho/base64 conforme sua estratégia
        public string? Arquivo { get; set; }
        public string? Senha { get; set; }
    }

    public class Csc
    {
        public string? Identificador { get; set; }
        public string? Valor { get; set; }
    }

    public class Emitente
    {
        public string? Cnpj { get; set; }
        public string? Cpf { get; set; }
        public string? InscricaoEstadual { get; set; }
        public string? RazaoSocial { get; set; }
        public string? Fantasia { get; set; }
        public Contato EmitenteContato { get; set; } = new();
        public Endereco EmitenteEndereco { get; set; } = new();
        public RegimeTributario RegimeTributario { get; set; } = new();
    }

    public class Contato
    {
        public string? Telefone { get; set; }
    }

    public class Endereco
    {
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? CodigoCidade { get; set; }
        public string? Cidade { get; set; }
        public string? Uf { get; set; }
    }

    public class RegimeTributario
    {
        public string? Crt { get; set; }
    }
}