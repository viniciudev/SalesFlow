using Model.Registrations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Moves
{
    public class PurchaseItem : BaseEntity
    {
        public int CompraId { get; set; }
        public Purchase Compra { get; set; }
        public int? ProdutoId { get; set; }
        public Product Produto { get; set; }
        public string CodigoProduto { get; set; }
        public string DescricaoProduto { get; set; }
        public decimal Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal Desconto { get; set; }
        public decimal ValorTotal { get; set; }

        [NotMapped]
        public string NomeProduto { get; set; }
    }
}
