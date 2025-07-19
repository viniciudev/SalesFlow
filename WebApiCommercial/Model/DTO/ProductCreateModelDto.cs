using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class ProductCreateModelDto
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public IFormFile Image { get; set; }
        public int Id { get; set; }
        //public int? IdCompany { get; set; }
    }
}
