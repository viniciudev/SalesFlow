using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Moves
{
	public class Sale : BaseEntity
	{
		public int IdCompany { get; set; }
		public Company Company { get; set; }
		public DateTime ReleaseDate { get; set; }
		public DateTime SaleDate { get; set; }
		public int? IdClient { get; set; }
		public Client Client { get; set; }
		public int? IdSeller { get; set; }
		public Salesman Salesman { get; set; }
		public decimal Total { get; set; }
		public ICollection<SaleItems> SaleItems { get; set; }
		[NotMapped]
		public string NameSeller { get; set; }
		[NotMapped]
		public string NameClient { get; set; }
		[NotMapped]
		public decimal ValueSale { get; set; }
		public ICollection<Financial> Financials { get; set; }
		public ICollection<NFeEmission> NFeEmissions { get; set; } = new List<NFeEmission>();
		public ICollection<SalePayment> SalePayments { get; set; } = new List<SalePayment>();
		public SaleStatus Status { get; set; }
		public bool SalesOrder { get; set; } = false;
	}
	public enum SaleStatus
	{
		completed = 0,
		canceled = 1,
		pending = 2,
			Approved = 3,
			Received = 4,
			Invoiced = 5,
	}
}
