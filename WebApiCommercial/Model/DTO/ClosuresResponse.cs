using Model.Closure;
using System;
using System.Collections.Generic;

namespace Model.DTO
{
	public class ClosuresResponse
	{
		public int Id { get; set; }
		public DateTime DateInit { get; set; }
		public decimal kilometerTraveled { get; set; }
		public DateTime DateFinal { get; set; }
		public ICollection<ClosuresDetail> ClosuresDetails { get; set; }
		public decimal ValueSumDetails { get; set; }
	}
}
