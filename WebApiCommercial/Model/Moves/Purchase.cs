using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Moves
{
    public class Purchase : BaseEntity
    {
        public int IdCompany { get; set; }
        public Company Company { get; set; }
        public DateTime DataEntrada { get; set; }
        public DateTime DataCompra { get; set; }
        public string ChaveNfe { get; set; }
        public int FornecedorId { get; set; }
        public Provider Fornecedor { get; set; }
        public decimal ValorTotal { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public ICollection<PurchaseItem> PurchaseItems { get; set; }

        [NotMapped]
        public string NomeFornecedor { get; set; }
    }
}
