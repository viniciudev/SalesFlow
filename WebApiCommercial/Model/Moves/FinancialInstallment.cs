using Model.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Moves
{
	public class FinancialInstallment : BaseEntity
	{
		public int FinancialId { get; set; }
		public int InstallmentNumber { get; set; }      // 1, 2, 3...
		public decimal Amount { get; set; }              // Valor da parcela
		public DateTime DueDate { get; set; }            // Data de vencimento
		public DateTime? PaymentDate { get; set; }       // Data de pagamento
		public FinancialStatus Status { get; set; }      // pending, paid, canceled
		public string? Barcode { get; set; }             // Código de barras do boleto
		public string? DigitableLine { get; set; }       // Linha digitável
		public int? PaymentMethodId { get; set; }        // Método usado (opcional)

		// Propriedades de navegação
		public virtual Financial Financial { get; set; }
		public virtual PaymentMethod? PaymentMethod { get; set; }
	}
}
