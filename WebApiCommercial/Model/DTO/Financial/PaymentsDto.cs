using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO.Financial
{
	public class PaymentsDto
	{
		public int PaymentMethodId { get; set; }
		public string PaymentMethodName { get; set; }
		public decimal Value { get; set; }
		
	}
}
