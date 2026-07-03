namespace Model.DTO
{
    public class ProductCreateModelDto
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string Code { get; set; }
        public string Reference { get; set; }
        public decimal CostPrice { get; set; }
        public string ?Observation { get; set; }
        public string Ncm { get; set; }

        // ===== MATRIZ TRIBUTARIA =====
        public int? SituacaoTributariaId { get; set; }

        // ===== TRIBUTACAO POR PRODUTO =====
        public bool UsaTributacaoPropria { get; set; } = false;
        public ProductTributacaoDto? ConfiguracaoTributaria { get; set; }
        public int? NaturezaOperacaoOrigemId { get; set; }
    }

    public class ProductTributacaoDto
    {
        public bool AplicarICMS { get; set; }
        public string? CstICMS { get; set; }
        public string? CsosnICMS { get; set; }
        public decimal AliquotaICMS { get; set; }
        public bool ReduzirBaseICMS { get; set; }

        public bool AplicarIPI { get; set; }
        public string? CstIPI { get; set; }
        public decimal AliquotaIPI { get; set; }

        public bool AplicarPIS { get; set; }
        public string? CstPIS { get; set; }
        public decimal AliquotaPIS { get; set; }

        public bool AplicarCOFINS { get; set; }
        public string? CstCOFINS { get; set; }
        public decimal AliquotaCOFINS { get; set; }

        public bool AplicarISSQN { get; set; }
        public decimal AliquotaISSQN { get; set; }

        public bool AplicarIBS { get; set; }
        public string? CstIBS { get; set; }
        public decimal AliquotaIBS { get; set; }

        public bool AplicarCBS { get; set; }
        public string? CstCBS { get; set; }
        public decimal AliquotaCBS { get; set; }

        public bool AplicarIS { get; set; }
        public decimal AliquotaIS { get; set; }

        public string? cClassTrib { get; set; }
    }
}
