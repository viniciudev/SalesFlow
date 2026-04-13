using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Moves
{
	
		public class FinancialPaymentMethod: BaseEntity
	{
	
			public int FinancialId { get; set; }
			public int PaymentMethodId { get; set; }

			/// <summary>
			/// Valor pago com este método de pagamento (opcional)
			/// Se não preenchido, considera-se o valor total do Financial
			/// </summary>
			[Column(TypeName = "decimal(18,2)")]
			public decimal Amount { get; set; }

			/// <summary>
			/// Parcelas (se for cartão de crédito, por exemplo)
			/// </summary>
			public int? Installments { get; set; }

			// Propriedades de navegação
			public virtual Financial Financial { get; set; }
			public virtual PaymentMethod PaymentMethod { get; set; }
		[NotMapped]
		public string PaymentMethodName { get; set; }
	}
	

}
