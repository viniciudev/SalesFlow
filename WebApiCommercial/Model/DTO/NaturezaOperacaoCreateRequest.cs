

using Model.Enums;
using Model.Registrations;

namespace Service.Dtos
{
    public class NaturezaOperacaoCreateRequest
    {
        public string Descricao { get; set; } = null!;
        public string Cfop { get; set; } = null!;
        public TipoDocumentoEnum TipoDocumento { get; set; }
        public FinalidadeEnum Finalidade { get; set; }
        public bool ConsumidorFinal { get; set; }
        public bool MovimentaEstoque { get; set; }
        public bool Ativo { get; set; } = true;
        public ConfiguracaoTributaria ConfiguracaoTributaria { get; set; } = new();
    }
}