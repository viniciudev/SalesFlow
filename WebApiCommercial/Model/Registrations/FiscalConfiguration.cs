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

        // NFSe / ServiceInvoice fields
        public int LastInvoiceNumber { get; set; }
        public string ?CodMunIBGE { get; set; }

        public Company Company { get; set; }
        public int CompanyId { get; set; }
    }

    public class NumeracaoDocumentos
    {
        public NumeracaoItem Nfe { get; set; } = new();
        public NumeracaoItem Nfce { get; set; } = new();

        /// <summary>
        /// Série da DPS (NFS-e padrão Nacional). É parte do IdDPS, junto com o
        /// município emissor, a inscrição federal e o número.
        /// </summary>
        public NumeracaoItem Dps { get; set; } = new();
    }

    public class NumeracaoItem
    {
        public string? Serie { get; set; }
        public long NumeroInicial { get; set; }
    }

    public class CertificadoDigital
    {
        // pode armazenar caminho/base64 conforme sua estrat�gia
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

        /// <summary>
        /// Inscrição Municipal do prestador. Obrigatória na DPS (prest/IM) — sem ela a
        /// emissão não tem como identificar o contribuinte perante o município.
        /// </summary>
        public string? InscricaoMunicipal { get; set; }

        public string? InscricaoEstadual { get; set; }
        public string? RazaoSocial { get; set; }
        public string? Fantasia { get; set; }
        public Contato EmitenteContato { get; set; } = new();
        public Endereco EmitenteEndereco { get; set; } = new();
        public RegimeTributario RegimeTributario { get; set; } = new();
        public byte[] ?Logo { get; set; }
    }

    public class Contato
    {
        public string? Telefone { get; set; }
        public string? Email { get; set; }
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

        /// <summary>
        /// Opção pelo Simples Nacional, como o padrão Nacional da NFS-e pede no grupo
        /// de tributação do prestador: 1 = Não optante, 2 = Optante MEI,
        /// 3 = Optante ME/EPP.
        ///
        /// É um campo separado do CRT de propósito: o CRT (1/2/3/4) diz o REGIME, mas
        /// não distingue MEI de ME/EPP, e o SEFIN exige essa distinção. Quando fica
        /// nulo, a emissão deriva do CRT — 1|2 → 3 (ME/EPP), 4 → 2 (MEI), resto → 1 —
        /// o que acerta o caso comum e fica documentado em vez de adivinhado.
        /// </summary>
        public int? OpcaoSimplesNacional { get; set; }
    }
}