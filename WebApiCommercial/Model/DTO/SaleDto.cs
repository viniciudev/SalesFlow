using Model.Moves;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
	public  class SaleDto
	{
		public int IdCompany { get; set; }

		public DateTime ReleaseDate { get; set; }
		public DateTime SaleDate { get; set; }
		public int? IdClient { get; set; }

		public int? IdSeller { get; set; }

		public decimal Total { get; set; }
		public ICollection<SaleItems> SaleItems { get; set; }
		[NotMapped]
		public string NameSeller { get; set; }
		[NotMapped]
		public string NameClient { get; set; }
		[NotMapped]
		public decimal ValueSale { get; set; }
		public ICollection<FormPaymentSale> FormPaymentSales { get; set; }
		public ICollection<NFeEmission> NFeEmissions { get; set; } = new List<NFeEmission>();
		public int Id { get; set; }
		public int? BankAccountId { get; set; }
	}
}
