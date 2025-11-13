using Microsoft.AspNetCore.Http;

namespace Model.DTO
{
    public class ProductCreateModelDto
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        //public IFormFile? Image { get; set; }
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string Code { get; set; }
        public string Reference { get; set; }
    }
}
