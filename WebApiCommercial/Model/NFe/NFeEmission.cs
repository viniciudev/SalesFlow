#nullable enable
using System;
using Model.Registrations;
using Model.Moves;
using Model.Enums;

namespace Model.Registrations
{
    public class NFeEmission : BaseEntity
    {
        public int NaturezaOperacaoId { get; set; }
        public NaturezaOperacao? NaturezaOperacao { get; set; }

        // Venda relacionada (opcional)
        public int SaleId { get; set; }
        public Sale? Sale { get; set; }

        // Dados de controle / retry
        public TipoDocumentoEnum TipoDocumento { get; set; } = TipoDocumentoEnum.NFE; // NFE ou NFCE
        public string? Serie { get; set; }
        public long? Numero { get; set; } // número emitido pela SEFAZ quando sucesso

        public bool Sent { get; set; } = false; // true quando emitido com sucesso
        public int TryCount { get; set; } = 0;

        // Payload e resposta (JSON armazenado para reprocessar / debug)
        public string? RequestPayloadJson { get; set; }
        public string? ResponseJson { get; set; }
        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Company? Company { get; set; }
        public int CompanyId { get; set; }
        public StatusNfe StatusNfe { get; set; }
    }
    public enum StatusNfe
    {
        pendente=1,
        emitida=2,
        cancelada=3,
        outros=4
    }
}