using Model.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    /// <summary>
    /// DTO para atualização de um serviço prestado (NFS-e 2026)
    /// </summary>
    public class ServiceProvidedUpdateRequest
    {
        // === DADOS BÁSICOS EXISTENTES ===

        [Required(ErrorMessage = "O nome do serviço é obrigatório")]
        [StringLength(120, ErrorMessage = "O nome deve ter no máximo 120 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O valor do serviço é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal Value { get; set; }

        public string Deadline { get; set; }
        public string Capacity { get; set; }
        public string Experience { get; set; }

        // === DADOS FISCAIS OBRIGATÓRIOS NFS-e 2026 ===

        [Required(ErrorMessage = "O código IBGE do município é obrigatório")]
        [RegularExpression(@"^\d{7}$", ErrorMessage = "O código IBGE deve ter exatamente 7 dígitos numéricos")]
        public string LocationCode { get; set; }

        [Required(ErrorMessage = "O código de tributação nacional é obrigatório")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "O código de tributação nacional deve ter exatamente 6 dígitos numéricos")]
        public string NationalTaxCode { get; set; }

        [Required(ErrorMessage = "A descrição do serviço é obrigatória")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre 10 e 2000 caracteres")]
        public string Description { get; set; }

        // === DADOS FISCAIS OPCIONAIS ===

        [RegularExpression(@"^\d{3}$", ErrorMessage = "O código de tributação municipal deve ter exatamente 3 dígitos numéricos")]
        public string MunicipalTaxCode { get; set; }

        [RegularExpression(@"^\d{9}$", ErrorMessage = "O código NBS deve ter exatamente 9 dígitos numéricos")]
        public string NbsCode { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9]{1,20}$", ErrorMessage = "O código interno deve ser alfanumérico e ter entre 1 e 20 caracteres")]
        public string InternalContributorCode { get; set; }

        public ServiceSpecialType SpecialType { get; set; } = ServiceSpecialType.None;

        // === CAMPOS PARA OBRA ===

        [StringLength(30, ErrorMessage = "A inscrição imobiliária fiscal deve ter no máximo 30 caracteres")]
        public string PropertyRegistry { get; set; }

        [StringLength(30, ErrorMessage = "O código da obra deve ter no máximo 30 caracteres")]
        public string ConstructionCode { get; set; }

        [RegularExpression(@"^\d{8}$", ErrorMessage = "O código CIB deve ter exatamente 8 dígitos numéricos")]
        public string CibCode { get; set; }

        // === CAMPOS PARA EVENTO ===

        [StringLength(255, ErrorMessage = "O nome do evento deve ter no máximo 255 caracteres")]
        public string EventName { get; set; }

        public DateTime? EventStartDate { get; set; }

        public DateTime? EventEndDate { get; set; }

        [StringLength(30, ErrorMessage = "O identificador do evento deve ter no máximo 30 caracteres")]
        public string EventIdentifier { get; set; }

        // === CAMPOS PARA COMÉRCIO EXTERIOR ===

        [RegularExpression(@"^[0-4]$", ErrorMessage = "O modo de prestação deve ser um valor entre 0 e 4")]
        public string ServiceMode { get; set; }

        [RegularExpression(@"^[0-69]$", ErrorMessage = "O vínculo deve ser um valor válido (0-6 ou 9)")]
        public string ServiceLink { get; set; }

        [RegularExpression(@"^\d{3}$", ErrorMessage = "O código da moeda deve ter exatamente 3 dígitos numéricos")]
        public string CurrencyCode { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "O valor em moeda estrangeira não pode ser negativo")]
        public decimal? ForeignValue { get; set; }
    }
}
