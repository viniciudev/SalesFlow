using Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    public class ServiceOrderCreateRequest
    {
        public int TenantId { get; set; }

        [Required(ErrorMessage = "Cliente é obrigatório")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Data da ordem é obrigatória")]
        public DateTime OrderDate { get; set; }

        /// <summary>Observações livres da OS.</summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Competência sugerida (mês de prestação). Serve de valor inicial para a
        /// DataCompetencia da NFS-e; não é a competência fiscal.
        /// </summary>
        public DateTime? Competence { get; set; }

        [Required(ErrorMessage = "Pelo menos um serviço é obrigatório")]
        [MinLength(1, ErrorMessage = "Pelo menos um serviço é obrigatório")]
        public List<ServiceOrderItemRequest> Items { get; set; } = new();

        /// <summary>
        /// Formas de pagamento da OS. Mesmo contrato usado por <c>SaleDto.FormPaymentSales</c>
        /// (wire: <c>formPaymentSales</c>). Opcional: uma OS pode ser registrada antes do acerto,
        /// e nesse caso nenhum lançamento financeiro é gerado.
        /// </summary>
        public ICollection<FormPaymentSale> FormPaymentSales { get; set; } = new List<FormPaymentSale>();
    }

    public class ServiceOrderUpdateRequest
    {
        public int ClientId { get; set; }
        public DateTime OrderDate { get; set; }
        public string? Notes { get; set; }
        public DateTime? Competence { get; set; }
        public List<ServiceOrderItemRequest> Items { get; set; } = new();

        /// <summary>
        /// Formas de pagamento da OS. No update a reconciliação é por regeneração: as parcelas
        /// pendentes são removidas e recriadas a partir desta lista; as já pagas viram
        /// <see cref="FinancialStatus.Canceled"/> em vez de serem apagadas.
        /// </summary>
        public ICollection<FormPaymentSale> FormPaymentSales { get; set; } = new List<FormPaymentSale>();
    }

    public class ServiceOrderItemRequest
    {
        [Required]
        public int ServiceProvidedId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor unitário deve ser maior que zero")]
        public decimal UnitPrice { get; set; }

        /// <summary>Desconto incondicional do item. Não pode ser negativo nem superar o bruto.</summary>
        [Range(0, double.MaxValue, ErrorMessage = "Desconto não pode ser negativo")]
        public decimal Discount { get; set; }

        /// <summary>Descrição que sobrepõe a do serviço cadastrado na DPS.</summary>
        [MaxLength(2000)]
        public string? Description { get; set; }

        [Range(0, 100, ErrorMessage = "Alíquota de ISS deve estar entre 0 e 100")]
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

    public class ServiceOrderResponse
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public DateTime OrderDate { get; set; }
        public string? Notes { get; set; }
        public DateTime? Competence { get; set; }

        /// <summary>Soma dos brutos dos itens, antes de descontos.</summary>
        public decimal TotalValue { get; set; }

        public decimal DiscountValue { get; set; }
        public decimal IssqnValue { get; set; }
        public decimal IssqnRetidoValue { get; set; }
        public decimal RetentionValue { get; set; }

        /// <summary>Líquido a receber: base menos retenções.</summary>
        public decimal NetValue { get; set; }

        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ConcludedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public List<ServiceOrderItemResponse> Items { get; set; } = new();
        public List<ServiceInvoiceBriefResponse> Invoices { get; set; } = new();

        /// <summary>
        /// Parcelas financeiras geradas a partir das formas de pagamento. Parcelas canceladas
        /// não são retornadas. Usado pela tela de edição para remontar o formulário de pagamento.
        /// </summary>
        public List<ServiceOrderFinancialResponse> Financials { get; set; } = new();
    }

    public class ServiceOrderFinancialResponse
    {
        public int Id { get; set; }
        public int PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; }
        public decimal Value { get; set; }
        public DateTime DueDate { get; set; }
        public FinancialStatus Status { get; set; }
    }

    public class ServiceOrderItemResponse
    {
        public int Id { get; set; }
        public int ServiceProvidedId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public string? Description { get; set; }

        /// <summary>Total do item já líquido do desconto: (Quantity x UnitPrice) - Discount.</summary>
        public decimal TotalPrice { get; set; }

        public decimal IssqnRate { get; set; }
        public bool IssqnRetido { get; set; }

        /// <summary>ISS do item, calculado no backend para a UI não reimplementar a regra.</summary>
        public decimal IssqnValue { get; set; }

        public decimal PisRate { get; set; }
        public decimal CofinsRate { get; set; }
        public decimal IrRate { get; set; }
        public decimal CsllRate { get; set; }
        public decimal InssRate { get; set; }

        public string LocationCode { get; set; }

        /// <summary>Código de tributação nacional do ISSQN (6 dígitos) — vai para a DPS.</summary>
        public string NationalTaxCode { get; set; }
    }

    public class ServiceInvoiceBriefResponse
    {
        public int Id { get; set; }
        public int NumeroDPS { get; set; }
        public string Status { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime? EmittedAt { get; set; }

        /// <summary>Chave de acesso da NFS-e (50 dígitos). Nula enquanto não autorizada.</summary>
        public string? ChaveAcesso { get; set; }

        /// <summary>true somente quando o SEFIN autorizou.</summary>
        public bool Sent { get; set; }

        /// <summary>Motivo da última falha de emissão, quando houver.</summary>
        public string? ErrorMessage { get; set; }
    }

    public class ServiceOrderStatusRequest
    {
        [Required]
        public ServiceOrderStatus Status { get; set; }
    }
}
