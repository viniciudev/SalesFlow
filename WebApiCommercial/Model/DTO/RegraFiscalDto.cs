using Model.Enums;
using Model.Registrations;

namespace Service.Dtos
{
    /// <summary>
    /// DTO para representar uma celula da matriz tributaria.
    /// </summary>
    public class RegraFiscalDto
    {
        public int? Id { get; set; }
        public int SituacaoTributariaId { get; set; }
        public string? SituacaoTributariaCodigo { get; set; }
        public string? SituacaoTributariaDescricao { get; set; }
        public Destino Destino { get; set; }
        public string Cfop { get; set; } = null!;
        public ConfiguracaoTributaria ConfiguracaoTributaria { get; set; } = new();
    }
}
