using System;

namespace Model.DTO
{
    /// <summary>
    /// Parcela financeira enviada pelo frontend na edição de uma venda.
    /// Id = 0 indica que o registro ainda não existe no banco (será criado).
    /// Id > 0 indica atualização do registro existente.
    /// </summary>
    public class SaleFinancialDto
    {
        /// <summary>Id do registro financeiro. 0 = novo registro.</summary>
        public int Id { get; set; }

        public int PaymentMethodId { get; set; }

        public decimal Value { get; set; }

        public DateTime DueDate { get; set; }

        /// <summary>
        /// Status atual da parcela (informativo). Para registros existentes o status
        /// é preservado do banco; para novos o backend calcula a partir do método de pagamento.
        /// </summary>
        public FinancialStatus Status { get; set; }

        public string Description { get; set; }

        public int? BankAccountId { get; set; }

        /// <summary>Número da parcela (ex.: 1 de 3).</summary>
        public int? InstallmentNumber { get; set; }

        /// <summary>Total de parcelas.</summary>
        public int? TotalInstallments { get; set; }
    }
}
