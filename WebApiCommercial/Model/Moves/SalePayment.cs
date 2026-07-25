using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

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

		/// <summary>
		/// Coluna persistida no banco (JSON) com as datas de vencimento de cada parcela.
		/// Ex.: ["2026-08-10T00:00:00","2026-09-25T00:00:00","2026-11-18T00:00:00"]
		/// </summary>
		public string? InstallmentDueDatesJson { get; set; }

		/// <summary>
		/// Propriedade de conveniencia (nao mapeada) para acessar as datas como lista.
		/// A serializacao/desserializacao e feita manualmente no service.
		/// </summary>
		[NotMapped]
		public List<DateTime>? InstallmentDueDates { get; set; }
	}

	public enum SalePaymentStatus
	{
		Planned = 0,
		Confirmed = 1,
		Cancelled = 2,
	}
}
