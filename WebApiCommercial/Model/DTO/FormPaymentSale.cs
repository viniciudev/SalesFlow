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
		public int Value { get; set; }
		public string PaymentMethodName { get; set; }
		public int? BankAccountId { get; set; }      // Nullable porque pode não existir
		public string? BankAccountName { get; set; }
	}
}
