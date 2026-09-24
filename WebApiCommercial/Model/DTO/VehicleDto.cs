using Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    /// <summary>
    /// Payload de criação/atualização de veículo.
    ///
    /// A validação é em duas camadas, de propósito:
    /// <list type="bullet">
    ///   <item>DataAnnotations aqui — pega ausência, tamanho e faixa. É o que o
    ///   <c>[ApiController]</c> já converte em 400 automaticamente.</item>
    ///   <item>Regras RV cruzadas no <c>VehicleService</c> — unicidade de placa
    ///   (RV02), UF existente (RV07), dados do proprietário (RV09/RV10). Essas
    ///   dependem do banco ou de outra entidade e não cabem em atributo.</item>
    /// </list>
    ///
    /// Os enums são ANULÁVEIS de propósito. Com tipo não anulável, "não
    /// informado" e o valor 0 ficariam indistinguíveis — e
    /// <see cref="BodyType.NaoAplicavel"/> é 0, um valor legítimo do MOC. Nulo +
    /// <c>[Required]</c> é o que separa "faltou" de "não aplicável".
    /// </summary>
    public class VehicleCreateDto
    {
        /// <summary>Código interno de frota. Opcional.</summary>
        [StringLength(30, ErrorMessage = "O código interno deve ter no máximo 30 caracteres.")]
        public string? InternalCode { get; set; }

        /// <summary>
        /// Placa, com ou sem separador — o serviço normaliza antes de gravar.
        /// O limite de 8 acomoda "ABC-1234"; a validação de formato em si é
        /// feita no serviço (RV01), depois da normalização, para dar uma
        /// mensagem que diz qual formato é aceito.
        /// </summary>
        [Required(ErrorMessage = "A placa é obrigatória.")]
        [StringLength(8, MinimumLength = 7, ErrorMessage = "A placa deve ter 7 caracteres (8 com separador).")]
        public string LicensePlate { get; set; } = string.Empty;

        /// <summary>RENAVAM — 11 dígitos quando informado (RV03).</summary>
        [StringLength(11, ErrorMessage = "O RENAVAM deve ter no máximo 11 dígitos.")]
        public string? Renavam { get; set; }

        [Required(ErrorMessage = "A UF de licenciamento é obrigatória.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "A UF deve ter 2 letras.")]
        public string LicensingState { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo de veículo é obrigatório.")]
        public VehicleType? VehicleType { get; set; }

        [Required(ErrorMessage = "O tipo de rodado é obrigatório.")]
        public WheelType? WheelType { get; set; }

        [Required(ErrorMessage = "O tipo de carroceria é obrigatório.")]
        public BodyType? BodyType { get; set; }

        /// <summary>
        /// Tara em KG (RV04). O piso 1 é a própria regra "maior que zero": o
        /// MOC rejeita tara zerada, e é melhor barrar aqui do que na SEFAZ.
        /// </summary>
        [Range(1, 999999, ErrorMessage = "A tara (KG) é obrigatória e deve ser maior que zero.")]
        public int Tare { get; set; }

        // [Range(0, 999999, ErrorMessage = "A capacidade (KG) deve estar entre 0 e 999999.")]
        public int? CapacityKg { get; set; }

        // [Range(typeof(decimal), "0", "999.99", ErrorMessage = "A capacidade (M³) deve estar entre 0 e 999,99.")]
        public decimal? CapacityM3 { get; set; }

        [Required(ErrorMessage = "O tipo de propriedade é obrigatório.")]
        public OwnershipType? OwnershipType { get; set; }

        /// <summary>
        /// Exigida apenas quando o veículo não é próprio (RV09) — por isso a
        /// obrigatoriedade é checada no serviço, e não com <c>[Required]</c>.
        /// </summary>
        public ThirdPartyCategory? ThirdPartyCategory { get; set; }

        /// <summary>
        /// Parceiro proprietário/locador (RV09). Obrigatório quando
        /// <see cref="OwnershipType"/> != Próprio.
        /// </summary>
        public int? ClientId { get; set; }
    }

    /// <summary>
    /// Atualização. Herda todas as regras de <see cref="VehicleCreateDto"/> e
    /// acrescenta o estado de ativação, que só faz sentido em um registro
    /// existente (RV11 — desativar/reativar é sempre lógico).
    /// </summary>
    public class VehicleUpdateDto : VehicleCreateDto
    {
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Dados do proprietário achatados para exibição — evita que o front-end
    /// precise de uma segunda chamada ao parceiro só para mostrar nome e
    /// documento na tela de detalhe.
    /// </summary>
    public class VehicleOwnerDto
    {
        public int ClientId { get; set; }
        public string? Name { get; set; }
        public string? Document { get; set; }
        public string? TipoPessoa { get; set; }
        public string? Ie { get; set; }
        public string? Uf { get; set; }
        public string? Rntrc { get; set; }
    }

    /// <summary>Detalhe completo do veículo (tela de edição/visualização).</summary>
    public class VehicleResponseDto
    {
        public int Id { get; set; }
        public int IdCompany { get; set; }
        public string? InternalCode { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string? Renavam { get; set; }
        public string LicensingState { get; set; } = string.Empty;
        public VehicleType VehicleType { get; set; }
        public WheelType WheelType { get; set; }
        public BodyType BodyType { get; set; }
        public int Tare { get; set; }
        public int? CapacityKg { get; set; }
        public decimal? CapacityM3 { get; set; }
        public OwnershipType OwnershipType { get; set; }
        public ThirdPartyCategory? ThirdPartyCategory { get; set; }
        public VehicleOwnerDto? Owner { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>Checklist de aptidão para MDF-e, já calculado (RV12/checklist da OS).</summary>
        public MdfEStatusResult MdfEStatus { get; set; } = new();
    }

    /// <summary>
    /// Linha da listagem paginada.
    ///
    /// Traz o status de MDF-e já resolvido porque a tabela tem uma coluna
    /// "Status MDF-e": sem isso, a tela precisaria de uma chamada por linha
    /// (N+1) só para pintar o badge.
    /// </summary>
    public class VehicleListItemDto
    {
        public int Id { get; set; }
        public string? InternalCode { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string LicensingState { get; set; } = string.Empty;
        public VehicleType VehicleType { get; set; }
        public WheelType WheelType { get; set; }
        public BodyType BodyType { get; set; }
        public int Tare { get; set; }
        public int? CapacityKg { get; set; }
        public decimal? CapacityM3 { get; set; }
        public OwnershipType OwnershipType { get; set; }
        public int? ClientId { get; set; }
        public string? ClientName { get; set; }
        public bool IsActive { get; set; }

        public bool IsReadyForMdfE { get; set; }

        /// <summary>
        /// Pendências que impedem o MDF-e. Preenchida também quando
        /// <see cref="IsReadyForMdfE"/> é true (nesse caso, vazia) — assim o
        /// front-end usa um único campo para decidir entre badge verde e âmbar.
        /// </summary>
        public List<string> MdfEPendingItems { get; set; } = new();
    }

    /// <summary>
    /// Resultado do checklist "Apto para MDF-e" (método
    /// <c>CalculateMdfEStatus</c>).
    ///
    /// <see cref="PendingItems"/> é preenchida com TODAS as pendências, não só a
    /// primeira: o objetivo é o usuário corrigir o cadastro em uma passada, em
    /// vez de descobrir um item por tentativa.
    /// </summary>
    public class MdfEStatusResult
    {
        public bool IsReady { get; set; }
        public List<string> PendingItems { get; set; } = new();
    }
}
