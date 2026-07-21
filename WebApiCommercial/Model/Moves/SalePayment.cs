using Model.Registrations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Moves
{
	public class SalePayment : BaseEntity
	{
		public int IdSale { get; set; }
		public Sale Sale { get; set; }
		public int PaymentMethodId { get; set; }
		public PaymentMethod PaymentMethod { get; set; }
		public decimal Value { get; set; }
		public int Installments { get; set; }
		public SalePaymentStatus Status { get; set; }
		[NotMapped]
		public string PaymentMethodName { get; set; }
	}

	public enum SalePaymentStatus
	{
		Planned = 0,
		Confirmed = 1,
		Cancelled = 2,
	}
}
