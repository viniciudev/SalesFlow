
namespace Model.Registrations
{
    public class ConfiguracaoTributaria
    {
        // ICMS
        public bool AplicarICMS { get; set; }
        public string? CstICMS { get; set; }
        public decimal AliquotaICMS { get; set; }
        public bool ReduzirBaseICMS { get; set; }

        // IPI
        public bool AplicarIPI { get; set; }
        public string? CstIPI { get; set; }
        public decimal AliquotaIPI { get; set; }

        // PIS
        public bool AplicarPIS { get; set; }
        public string? CstPIS { get; set; }
        public decimal AliquotaPIS { get; set; }

        // COFINS
        public bool AplicarCOFINS { get; set; }
        public string? CstCOFINS { get; set; }
        public decimal AliquotaCOFINS { get; set; }

        // ISSQN
        public bool AplicarISSQN { get; set; }
        public decimal AliquotaISSQN { get; set; }

        // IBS
        public bool AplicarIBS { get; set; }
        public string? CstIBS { get; set; }
        public decimal AliquotaIBS { get; set; }

        // CBS
        public bool AplicarCBS { get; set; }
        public string? CstCBS { get; set; }
        public decimal AliquotaCBS { get; set; }

        // IS
        public bool AplicarIS { get; set; }
        public decimal AliquotaIS { get; set; }
    }
}