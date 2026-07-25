using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
	public class FormPaymentSale
	{
		public int PaymentMethodId { get; set; }
		public decimal Value { get; set; }
		public string PaymentMethodName { get; set; }
		public int? BankAccountId { get; set; }      // Nullable porque pode não existir
		public string? BankAccountName { get; set; }
		public int? Installments { get; set; }
		/// <summary>
		/// Datas de vencimento manuais para cada parcela.
		/// Quando informado, substitui o cálculo automático de vencimento.
		/// Deve ter o mesmo número de elementos que Installments.
		/// </summary>
		public List<DateTime>? InstallmentDueDates { get; set; }
	}
}
