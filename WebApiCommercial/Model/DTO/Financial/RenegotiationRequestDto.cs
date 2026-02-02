using System;
using System.Collections.Generic;

namespace YourNamespace.DTOs
{
    public class RenegotiationRequestDto
    {
        /// <summary>
        /// IDs das parcelas originais que serão renegociadas
        /// </summary>
        public List<int> OriginalInstallments { get; set; } = new List<int>();

        /// <summary>
        /// ID do cliente
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Descrição/observação da renegociação
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Novo valor total após renegociação (com descontos/juros aplicados)
        /// </summary>
        public decimal NewValue { get; set; }

        /// <summary>
        /// Número de parcelas da nova renegociação
        /// </summary>
        public int NumberOfInstallments { get; set; }

        /// <summary>
        /// Data de vencimento da primeira parcela da renegociação
        /// </summary>
        public DateTime NewDueDate { get; set; }

        /// <summary>
        /// Valor original total das parcelas selecionadas (antes de qualquer ajuste)
        /// </summary>
        public decimal OriginalTotalValue { get; set; }

        /// <summary>
        /// Indica se juros e multas foram aplicados
        /// </summary>
        public bool ApplyFees { get; set; }

        /// <summary>
        /// Percentual de desconto aplicado (0-100)
        /// </summary>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Valor total do desconto aplicado
        /// </summary>
        public decimal? DiscountValue { get; set; }

        /// <summary>
        /// Percentual de multa por atraso (0-100)
        /// </summary>
        public decimal LateFeePercentage { get; set; }

        /// <summary>
        /// Valor total da multa por atraso
        /// </summary>
        public decimal? LateFeeValue { get; set; }

        /// <summary>
        /// Percentual de juros ao dia (0-100)
        /// </summary>
        public decimal? InterestPercentage { get; set; }

        /// <summary>
        /// Número de dias em atraso considerados para cálculo de juros
        /// </summary>
        public int? InterestDays { get; set; }

        /// <summary>
        /// Valor total dos juros
        /// </summary>
        public decimal? InterestValue { get; set; }

        /// <summary>
        /// Valor total de todas as taxas (multa + juros)
        /// </summary>
        public decimal TotalFees { get; set; }

        /// <summary>
        /// Tipo de renegociação
        /// </summary>
        public RenegotiationType? Type { get; set; } = RenegotiationType.Consolidation;

        /// <summary>
        /// Status da renegociação
        /// </summary>
        public RenegotiationStatus? Status { get; set; } = RenegotiationStatus.Pending;

        /// <summary>
        /// Data de criação da solicitação de renegociação
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID do usuário que solicitou a renegociação
        /// </summary>
        public int? CreatedByUserId { get; set; }

        /// <summary>
        /// Detalhes das novas parcelas (preenchido após cálculo no backend)
        /// </summary>
        public List<NewInstallmentDto>? NewInstallments { get; set; } = new List<NewInstallmentDto>();

        /// <summary>
        /// Detalhes das parcelas originais (para auditoria)
        /// </summary>
        public List<OriginalInstallmentDetailDto> OriginalInstallmentsDetails { get; set; } = new List<OriginalInstallmentDetailDto>();
        public int IdCompany { get; set; }
        public int? PaymentMethodId { get; set; }
    }

    public class NewInstallmentDto
    {
        /// <summary>
        /// Número da parcela (1, 2, 3...)
        /// </summary>
        public int InstallmentNumber { get; set; }

        /// <summary>
        /// Valor da parcela
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Data de vencimento da parcela
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Descrição da parcela
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Status da nova parcela
        /// </summary>
        public FinancialStatus Status { get; set; } = FinancialStatus.pending;

        /// <summary>
        /// Indica se esta parcela tem juros/multas embutidos
        /// </summary>
        public bool HasFeesIncluded { get; set; }
    }

    public class OriginalInstallmentDetailDto
    {
        public int FinancialId { get; set; }
        public string Description { get; set; }
        public decimal OriginalValue { get; set; }
        public DateTime OriginalDueDate { get; set; }
        public FinancialStatus OriginalStatus { get; set; }
        public int DaysLate { get; set; }
        public decimal CalculatedLateFee { get; set; }
        public decimal CalculatedInterest { get; set; }
    }

    public enum RenegotiationType
    {
        /// <summary>
        /// Renegociação de uma única parcela
        /// </summary>
        SingleInstallment = 1,

        /// <summary>
        /// Consolidação de múltiplas parcelas em uma nova
        /// </summary>
        Consolidation = 2,

        /// <summary>
        /// Parcelamento de uma dívida existente
        /// </summary>
        InstallmentPlan = 3,

        /// <summary>
        /// Renegociação com desconto
        /// </summary>
        Discount = 4
    }

    public enum RenegotiationStatus
    {
        /// <summary>
        /// Aguardando aprovação
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Aprovada e processada
        /// </summary>
        Approved = 2,

        /// <summary>
        /// Rejeitada
        /// </summary>
        Rejected = 3,

        /// <summary>
        /// Cancelada pelo usuário
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// Em análise
        /// </summary>
        UnderReview = 5
    }

    public class RenegotiationResponseDto
    {
        public int RenegotiationId { get; set; }
        public string ProtocolNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public RenegotiationStatus Status { get; set; }
        public string StatusDescription { get; set; }
        public decimal OriginalTotalValue { get; set; }
        public decimal NewTotalValue { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalFees { get; set; }
        public int NumberOfInstallments { get; set; }
        public List<NewInstallmentDto> NewInstallments { get; set; } = new List<NewInstallmentDto>();
        public List<RenegotiationHistoryDto> History { get; set; } = new List<RenegotiationHistoryDto>();
    }

    public class RenegotiationHistoryDto
    {
        public DateTime Date { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public class RenegotiationCalculationRequestDto
    {
        public List<int> InstallmentIds { get; set; } = new List<int>();
        public decimal DiscountPercentage { get; set; }
        public decimal LateFeePercentage { get; set; }
        public decimal InterestPercentagePerDay { get; set; }
        public int NumberOfNewInstallments { get; set; } = 1;
        public DateTime NewFirstDueDate { get; set; }
    }

    public class RenegotiationCalculationResponseDto
    {
        public decimal OriginalTotalValue { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal LateFeeValue { get; set; }
        public decimal InterestValue { get; set; }
        public decimal TotalFees { get; set; }
        public decimal NewTotalValue { get; set; }
        public decimal InstallmentValue { get; set; }
        public List<InstallmentPreviewDto> InstallmentsPreview { get; set; } = new List<InstallmentPreviewDto>();
    }

    public class InstallmentPreviewDto
    {
        public int InstallmentNumber { get; set; }
        public decimal Value { get; set; }
        public DateTime DueDate { get; set; }
        public string Description { get; set; }
    }
}