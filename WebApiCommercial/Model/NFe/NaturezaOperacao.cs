using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.Registrations
{
    public class NaturezaOperacao:BaseEntity
    {
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        [Required]
        [MaxLength(150)]
        public string Descricao { get; set; } = null!;

        /// <summary>
        /// CFOP padrao (fallback). Mantido para compatibilidade com registros existentes.
        /// Quando existirem RegrasFiscais configuradas, o CFOP sera resolvido pela matriz.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string Cfop { get; set; } = null!;

        public Enums.TipoDocumentoEnum TipoDocumento { get; set; }

        public Enums.FinalidadeEnum Finalidade { get; set; }

        public bool ConsumidorFinal { get; set; }

        public bool MovimentaEstoque { get; set; }

        public bool Ativo { get; set; } = true;

        /// <summary>
        /// Permite que produtos vinculados a esta natureza tenham tributação própria.
        /// Deprecated: Substituido pela matriz tributaria (RegraFiscal).
        /// Mantido para compatibilidade com registros existentes.
        /// </summary>
        public bool PermiteTributacaoPorProduto { get; set; } = false;

        /// <summary>
        /// Configuracao tributaria padrao (fallback).
        /// Mantida para compatibilidade com registros existentes.
        /// Quando existirem RegrasFiscais, a matriz tem prioridade.
        /// </summary>
        public ConfiguracaoTributaria ConfiguracaoTributaria { get; set; } = new();

        /// <summary>
        /// Matriz tributaria: conjunto de regras fiscais que combinam
        /// SituacaoTributaria + Destino para determinar CFOP e tributos por item.
        /// </summary>
        public ICollection<RegraFiscal> RegrasFiscais { get; set; } = new List<RegraFiscal>();

        public ICollection<NFeEmission> NFeEmissions { get; set; }= new List<NFeEmission>();
    }
}
