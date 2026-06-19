using Model.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    /// <summary>
    /// DTO para criação de um novo serviço prestado (NFS-e 2026)
    /// </summary>
    public class ServiceProvidedCreateRequest
    {
        public int IdCompany { get; set; }

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

        /// <summary>
        /// Código IBGE do município de prestação - 7 dígitos
        /// </summary>
        [Required(ErrorMessage = "O código IBGE do município é obrigatório")]
        [RegularExpression(@"^\d{7}$", ErrorMessage = "O código IBGE deve ter exatamente 7 dígitos numéricos")]
        public string LocationCode { get; set; }

        /// <summary>
        /// Código de tributação nacional ISSQN - 6 dígitos
        /// </summary>
        [Required(ErrorMessage = "O código de tributação nacional é obrigatório")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "O código de tributação nacional deve ter exatamente 6 dígitos numéricos")]
        public string NationalTaxCode { get; set; }

        /// <summary>
        /// Descrição completa do serviço - mínimo 10, máximo 2000 caracteres
        /// </summary>
        [Required(ErrorMessage = "A descrição do serviço é obrigatória")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre 10 e 2000 caracteres")]
        public string Description { get; set; }

        // === DADOS FISCAIS OPCIONAIS ===

        /// <summary>
        /// Código de tributação municipal - opcional, 3 dígitos
        /// </summary>
        [RegularExpression(@"^\d{3}$", ErrorMessage = "O código de tributação municipal deve ter exatamente 3 dígitos numéricos")]
        public string MunicipalTaxCode { get; set; }

        /// <summary>
        /// Código NBS - opcional, 9 dígitos
        /// </summary>
        [RegularExpression(@"^\d{9}$", ErrorMessage = "O código NBS deve ter exatamente 9 dígitos numéricos")]
        public string NbsCode { get; set; }

        /// <summary>
        /// Código interno do contribuinte - opcional, até 20 caracteres
        /// </summary>
        [RegularExpression(@"^[a-zA-Z0-9]{1,20}$", ErrorMessage = "O código interno deve ser alfanumérico e ter entre 1 e 20 caracteres")]
        public string InternalContributorCode { get; set; }

        /// <summary>
        /// Tipo especial de serviço
        /// </summary>
        public ServiceSpecialType SpecialType { get; set; } = ServiceSpecialType.None;

        // === CAMPOS PARA OBRA ===

        /// <summary>
        /// Inscrição imobiliária fiscal
        /// </summary>
        [StringLength(30, ErrorMessage = "A inscrição imobiliária fiscal deve ter no máximo 30 caracteres")]
        public string PropertyRegistry { get; set; }

        /// <summary>
        /// Código da obra (CNO/CEI)
        /// </summary>
        [StringLength(30, ErrorMessage = "O código da obra deve ter no máximo 30 caracteres")]
        public string ConstructionCode { get; set; }

        /// <summary>
        /// Código CIB
        /// </summary>
        [RegularExpression(@"^\d{8}$", ErrorMessage = "O código CIB deve ter exatamente 8 dígitos numéricos")]
        public string CibCode { get; set; }

        // === CAMPOS PARA EVENTO ===

        /// <summary>
        /// Nome do evento
        /// </summary>
        [StringLength(255, ErrorMessage = "O nome do evento deve ter no máximo 255 caracteres")]
        public string EventName { get; set; }

        /// <summary>
        /// Data de início do evento
        /// </summary>
        public DateTime? EventStartDate { get; set; }

        /// <summary>
        /// Data de fim do evento
        /// </summary>
        public DateTime? EventEndDate { get; set; }

        /// <summary>
        /// Identificador do evento
        /// </summary>
        [StringLength(30, ErrorMessage = "O identificador do evento deve ter no máximo 30 caracteres")]
        public string EventIdentifier { get; set; }

        // === CAMPOS PARA COMÉRCIO EXTERIOR ===

        /// <summary>
        /// Modo de prestação (0-4)
        /// </summary>
        [RegularExpression(@"^[0-4]$", ErrorMessage = "O modo de prestação deve ser um valor entre 0 e 4")]
        public string ServiceMode { get; set; }

        /// <summary>
        /// Vínculo entre as partes (0-6, 9)
        /// </summary>
        [RegularExpression(@"^[0-69]$", ErrorMessage = "O vínculo deve ser um valor válido (0-6 ou 9)")]
        public string ServiceLink { get; set; }

        /// <summary>
        /// Código da moeda (BACEN) - 3 dígitos
        /// </summary>
        [RegularExpression(@"^\d{3}$", ErrorMessage = "O código da moeda deve ter exatamente 3 dígitos numéricos")]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Valor em moeda estrangeira
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "O valor em moeda estrangeira não pode ser negativo")]
        public decimal? ForeignValue { get; set; }
    }
}
