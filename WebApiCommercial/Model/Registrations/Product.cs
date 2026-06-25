using Model.Moves;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Registrations
{
    public class Product : BaseEntity
    {
        public int IdCompany { get; set; }
        public Company Company { get; set; }
        [Uppercase]
        public string Name { get; set; }
        public decimal Value { get; set; }
        public decimal Quantity { get; set; }
        public string Description { get; set; }
        public string Observation { get; set; } = string.Empty;
        public byte[] ?Image { get; set; }
        public ICollection<SaleItems> SaleItems { get; set; }
        public ICollection<Commission> Commissions { get; set; }
        public ICollection<Financial> Financials { get; set; }
        public ICollection<Stock> Stocks { get; set; }= new List<Stock>();

        public string Code { get; set; }
        public string ImageBytes { get; set; }
        public string Reference { get; set; }
        public decimal CostPrice { get; set; }
        public string Ncm { get; set; }
        public ICollection<PurchaseItem> PurchaseItems { get; set; }

        // ========== TRIBUTAÇÃO POR PRODUTO ==========

        /// <summary>
        /// Quando true, este produto usa sua própria ConfiguracaoTributaria.
        /// Quando false, herda da NaturezaOperacao da venda (default).
        /// </summary>
        public bool UsaTributacaoPropria { get; set; } = false;

        /// <summary>
        /// Configuração tributária específica do produto.
        /// Só é utilizada quando UsaTributacaoPropria = true.
        /// </summary>
        public ConfiguracaoTributaria? ConfiguracaoTributaria { get; set; }

        /// <summary>
        /// ID da Natureza de Operação de origem (quando a config foi clonada).
        /// Null se a tributação foi configurada manualmente.
        /// </summary>
        public int? NaturezaOperacaoOrigemId { get; set; }

        /// <summary>
        /// Data da última modificação da configuração tributária.
        /// </summary>
        public DateTime? DataAtualizacaoTributaria { get; set; }
    }
}
