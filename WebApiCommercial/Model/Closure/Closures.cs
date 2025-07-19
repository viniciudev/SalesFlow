using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.Closure
{
	public class Closures : BaseEntity
	{
		public string LongInit { get; set; }
		public string LatInit { get; set; }
		public DateTime DateInit { get; set; }
		public Type Type { get; set; }
		public string LongFinal { get; set; }
		public string LatFinal { get; set; }
		[DisplayFormat(DataFormatString = "{0:0,0.000000}")]
		public decimal kilometerTraveled { get; set; }
		public decimal Odometer { get; set; }
		public DateTime DateFinal { get; set; }
		public ICollection<ClosuresDetail> ClosuresDetails { get; set; }
		public int IdSalesman { get; set; }
		public Salesman Salesman { get; set; }
		public decimal OdometerFinal { get; set; }
	}

}
public enum Type
{
	CheckIn,
	CheckOut
}
