using Model.Enums;
using System;

namespace Model.DTO
{
    /// <summary>
    /// DTO de resposta para serviço prestado (NFS-e 2026)
    /// </summary>
    public class ServiceProvidedResponse
    {
        public int Id { get; set; }
        public int IdCompany { get; set; }

        // Dados básicos
        public string Name { get; set; }
        public decimal Value { get; set; }
        public string Deadline { get; set; }
        public string Capacity { get; set; }
        public string Experience { get; set; }

        // Dados fiscais obrigatórios
        public string LocationCode { get; set; }
        public string NationalTaxCode { get; set; }
        public string Description { get; set; }

        // Dados fiscais opcionais
        public string MunicipalTaxCode { get; set; }
        public string NbsCode { get; set; }
        public string InternalContributorCode { get; set; }

        // Tipo especial
        public string SpecialType { get; set; }

        // Campos de obra
        public string PropertyRegistry { get; set; }
        public string ConstructionCode { get; set; }
        public string CibCode { get; set; }

        // Campos de evento
        public string EventName { get; set; }
        public DateTime? EventStartDate { get; set; }
        public DateTime? EventEndDate { get; set; }
        public string EventIdentifier { get; set; }

        // Campos de comércio exterior
        public string ServiceMode { get; set; }
        public string ServiceLink { get; set; }
        public string CurrencyCode { get; set; }
        public decimal? ForeignValue { get; set; }
    }
}
