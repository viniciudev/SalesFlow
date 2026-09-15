using Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    public class ServiceInvoiceCreateRequest
    {
        [Required(ErrorMessage = "Ordem de serviço é obrigatória")]
        public int ServiceOrderId { get; set; }

        public int TenantId { get; set; }

        [Required(ErrorMessage = "Data de competência é obrigatória")]
        public DateTime DataCompetencia { get; set; }

        public AmbienteEnum TipoAmbiente { get; set; } = AmbienteEnum.Producao;
    }

    public class ServiceInvoiceUpdateRequest
    {
        public DateTime DataCompetencia { get; set; }
        public AmbienteEnum TipoAmbiente { get; set; }
        public string CodMunIBGE { get; set; }
        public List<ServiceInvoiceItemRequest> Items { get; set; } = new();
    }

    public class ServiceInvoiceItemRequest
    {
        [Required]
        public int ServiceProvidedId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Discount { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Range(0, 100)]
        public decimal IssqnRate { get; set; }

        public bool IssqnRetido { get; set; }

        [Range(0, 100)]
        public decimal PisRate { get; set; }

        [Range(0, 100)]
        public decimal CofinsRate { get; set; }

        [Range(0, 100)]
        public decimal IrRate { get; set; }

        [Range(0, 100)]
        public decimal CsllRate { get; set; }

        [Range(0, 100)]
        public decimal InssRate { get; set; }
    }

    public class ServiceInvoiceResponse
    {
        public int Id { get; set; }
        public int ServiceOrderId { get; set; }
        public int TenantId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientDocument { get; set; }

        /// <summary>
        /// Identificador da DPS no padrão Nacional. Fica nulo até a emissão — não é
        /// mais gerado na criação (o valor antigo era um GUID truncado, que não é um id de DPS).
        /// </summary>
        public string? IdDPS { get; set; }

        /// <summary>Chave de acesso da NFS-e (50 dígitos). Nula enquanto não autorizada.</summary>
        public string? ChaveAcesso { get; set; }

        public string TipoAmbiente { get; set; }
        public DateTime? DhEmissao { get; set; }
        public string CodMunIBGE { get; set; }
        public string Status { get; set; }
        public int NumeroDPS { get; set; }
        public DateTime DataCompetencia { get; set; }

        /// <summary>Soma dos brutos dos itens, antes de descontos.</summary>
        public decimal TotalValue { get; set; }

        public decimal DiscountValue { get; set; }
        public decimal IssqnValue { get; set; }
        public decimal IssqnRetidoValue { get; set; }
        public decimal RetentionValue { get; set; }

        /// <summary>Líquido a receber. Conceito nosso (financeiro/DANFSe) — não vai na DPS.</summary>
        public decimal NetValue { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? EmittedAt { get; set; }
        public string CancelReason { get; set; }
        public Guid? CanceledBy { get; set; }
        public Guid CreatedBy { get; set; }

        // === Retorno da transmissão ===
        //
        // XmlNfse NÃO sai daqui: é coluna "text" e estouraria o payload de uma listagem
        // paginada. Quem precisa do XML usa GET /api/serviceinvoices/{id}/xml. Mas o
        // ServiceInvoiceResponse também serve a listagem, então a presença do XML é
        // sinalizada por HasXml e não pelo conteúdo.

        /// <summary>true quando existe XML autorizado gravado.</summary>
        public bool HasXml { get; set; }

        /// <summary>true somente quando o SEFIN autorizou.</summary>
        public bool Sent { get; set; }

        /// <summary>Quantas vezes a emissão foi tentada.</summary>
        public int TryCount { get; set; }

        /// <summary>Motivo da última falha de emissão, quando houver.</summary>
        public string? ErrorMessage { get; set; }

        public List<ServiceInvoiceItemResponse> Items { get; set; } = new();
    }

    public class ServiceInvoiceItemResponse
    {
        public int Id { get; set; }
        public int ServiceProvidedId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public string? Description { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal IssqnRate { get; set; }
        public bool IssqnRetido { get; set; }

        /// <summary>ISS do item, calculado no backend.</summary>
        public decimal IssqnValue { get; set; }

        public decimal PisRate { get; set; }
        public decimal CofinsRate { get; set; }
        public decimal IrRate { get; set; }
        public decimal CsllRate { get; set; }
        public decimal InssRate { get; set; }
        public string LocationCode { get; set; }
        public string NationalTaxCode { get; set; }
    }

    public class ServiceInvoiceStatusRequest
    {
        [Required]
        public ServiceInvoiceStatus Status { get; set; }

        public string CancelReason { get; set; }
    }

    public class ServiceInvoiceCancelRequest
    {
        [Required(ErrorMessage = "Justificativa de cancelamento é obrigatória")]
        [MinLength(15, ErrorMessage = "Justificativa deve ter no mínimo 15 caracteres")]
        [MaxLength(500, ErrorMessage = "Justificativa deve ter no máximo 500 caracteres")]
        public string CancelReason { get; set; }
    }

    public class AvailableServiceResponse
    {
        public int ServiceProvidedId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
        public string LocationCode { get; set; }
        public string NationalTaxCode { get; set; }

        // Necessários para montar a DPS: sem eles a emissão não tem como preencher
        // o grupo de tributação municipal nem escolher o subgrupo (obra/evento/exterior).
        public string? MunicipalTaxCode { get; set; }
        public string? NbsCode { get; set; }
        public ServiceSpecialType? SpecialType { get; set; }
    }
}
