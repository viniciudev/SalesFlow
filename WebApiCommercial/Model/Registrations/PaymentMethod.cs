


using Model.Moves;
using System.Collections.Generic;

namespace Model.Registrations
{
	public class PaymentMethod : BaseEntity
	{
		public string Name { get; set; }
		public int IdCompany { get; set; }
		public Company Company { get; set; }

		public bool AllowInstallments { get; set; } = false; // Permite parcelamento?
		public bool IsImmediateSettlement { get; set; } = true; // 
		public virtual ICollection<FinancialPaymentMethod> FinancialPaymentMethods { get; set; } = new List<FinancialPaymentMethod>();
				
	}
}
