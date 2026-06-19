using Model.Enums;
using Model.Moves;
using System;
using System.Collections.Generic;

namespace Model.Registrations
{
    public class ServiceProvided : BaseEntity
    {
        public int IdCompany { get; set; }
        public Company Company { get; set; }
        public string? Name { get; set; }
        public decimal Value { get; set; }
        //public string Deadline { get; set; }
        //public string Capacity { get; set; }
        //public string Experience { get; set; }

        // === NOVOS CAMPOS NFS-e 2026 (DPS - grupo serv) ===

        /// <summary>
        /// Código do município onde o serviço foi prestado (tabela IBGE) - 7 dígitos numéricos
        /// </summary>
        public string LocationCode { get; set; }

        /// <summary>
        /// Código de tributação nacional do ISSQN - 6 dígitos numéricos
        /// Regra: 2 dígitos Item + 2 dígitos Subitem + 2 dígitos Desdobro Nacional (LC 116/2003)
        /// </summary>
        public string NationalTaxCode { get; set; }

        /// <summary>
        /// Descrição completa do serviço prestado - até 2000 caracteres
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Código de tributação municipal do ISSQN - opcional, 3 dígitos
        /// </summary>
        public string? MunicipalTaxCode { get; set; }

        /// <summary>
        /// Código NBS (Nomenclatura Brasileira de Serviços) - opcional, 9 dígitos
        /// </summary>
        public string? NbsCode { get; set; }

        /// <summary>
        /// Código interno do contribuinte - opcional, até 20 caracteres alfanuméricos
        /// </summary>
        public string? InternalContributorCode { get; set; }

        /// <summary>
        /// Tipo especial de serviço: None, Construction (obra), Event (evento), ForeignTrade (comércio exterior)
        /// </summary>
        public ServiceSpecialType ?SpecialType { get; set; }

        // === CAMPOS PARA SERVIÇOS DE OBRA (SpecialType = Construction) ===

        /// <summary>
        /// Inscrição imobiliária fiscal - opcional, até 30 caracteres
        /// </summary>
        public string? PropertyRegistry { get; set; }

        /// <summary>
        /// Código da obra: CNO (Cadastro Nacional de Obras) ou CEI (Cadastro Específico do INSS) - até 30 caracteres
        /// </summary>
        public string? ConstructionCode { get; set; }

        /// <summary>
        /// Código do Cadastro Imobiliário Brasileiro (CIB) - 8 dígitos
        /// </summary>
        public string? CibCode { get; set; }

        // === CAMPOS PARA SERVIÇOS DE EVENTO (SpecialType = Event) ===

        /// <summary>
        /// Nome/Descrição do evento artístico, cultural, esportivo, etc.
        /// </summary>
        public string? EventName { get; set; }

        /// <summary>
        /// Data de início da atividade de evento
        /// </summary>
        public DateTime? EventStartDate { get; set; }

        /// <summary>
        /// Data de fim da atividade de evento
        /// </summary>
        public DateTime? EventEndDate { get; set; }

        /// <summary>
        /// Identificação da Atividade de Evento (código determinado pela Administração Tributária Municipal) - até 30 caracteres
        /// </summary>
        public string? EventIdentifier { get; set; }

        // === CAMPOS PARA COMÉRCIO EXTERIOR (SpecialType = ForeignTrade) ===

        /// <summary>
        /// Modo de Prestação: 0-Desconhecido, 1-Transfronteiriço, 2-Consumo no Brasil, 3-Presença Comercial no Exterior, 4-Movimento Temporário
        /// </summary>
        public string? ServiceMode { get; set; }

        /// <summary>
        /// Vínculo entre as partes: 0-Sem vínculo, 1-Controlada, 2-Controladora, 3-Coligada, 4-Matriz, 5-Filial, 6-Outro, 9-Desconhecido
        /// </summary>
        public string? ServiceLink { get; set; }

        /// <summary>
        /// Código da moeda da transação comercial (código BACEN) - 3 dígitos
        /// </summary>
        public string? CurrencyCode { get; set; }

        /// <summary>
        /// Valor do serviço prestado expresso em moeda estrangeira
        /// </summary>
        public decimal? ForeignValue { get; set; }

        // === RELACIONAMENTOS EXISTENTES ===
        public ICollection<DetailsService> Details { get; set; }
        public ICollection<SaleItems> SaleItems { get; set; }
        public ICollection<Commission> Commissions { get; set; }
        public ICollection<Financial> Financials { get; set; }
    }
}
