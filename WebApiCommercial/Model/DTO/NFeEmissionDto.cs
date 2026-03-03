using Model.Enums;
using Model.Registrations;
using System;

namespace Model.DTO
{
    public class NFeEmissionDto
    {
        public int NaturezaOperacaoId { get; set; }

        public int SaleId { get; set; }



        public TipoDocumentoEnum TipoDocumento { get; set; } = TipoDocumentoEnum.NFE; // NFE ou NFCE
        public string? Serie { get; set; }
        public long? Numero { get; set; } // número emitido pela SEFAZ quando sucesso


        public StatusNfe StatusNfe { get; set; } = StatusNfe.pendente;
        public DateTime CreatedAt { get; set; }
        public int TryCount { get; set; }
        public int Id { get; set; }
        public int CompanyId { get; set; }
    }
}
