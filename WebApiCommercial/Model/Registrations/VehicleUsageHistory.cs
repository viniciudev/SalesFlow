using Model.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Model.Registrations
{
    /// <summary>
    /// Histórico de uso de um veículo (RV14): em qual MDF-e ou Ordem de Serviço
    /// ele foi associado, e quando.
    ///
    /// Por que esta tabela existe: a RV13 proíbe FK fixa motorista × veículo —
    /// a composição (veículo + condutor) é decidida a cada viagem. Sem uma FK,
    /// não sobra rastro de "quem rodou com o quê"; este histórico é esse rastro.
    ///
    /// É a PRIMEIRA tabela de histórico do projeto — não havia padrão de
    /// auditoria a seguir (o mais próximo é o snapshot em
    /// <c>NFeEmission.TributacaoAuditJson</c> e as colunas
    /// <c>CreatedAt/CreatedBy</c> de <c>ServiceOrder</c>). O desenho segue a
    /// tabela-filha <see cref="DriverLicense"/>: Id próprio, coluna de FK e
    /// navegação para o pai.
    ///
    /// Nasce VAZIA nesta entrega (a OS pede "preparada, mesmo que populada
    /// depois"): a emissão de MDF-e ainda não existe. Quem for gravar aqui deve
    /// preencher <see cref="IdCompany"/> a partir do veículo, para que o
    /// histórico respeite o isolamento por empresa.
    /// </summary>
    public class VehicleUsageHistory : BaseEntity
    {
        public int IdVehicle { get; set; }
        public Vehicle Vehicle { get; set; }

        /// <summary>
        /// Empresa, redundante com <see cref="Vehicle"/> de propósito: permite
        /// consultar o histórico filtrando por empresa sem depender de join, e
        /// mantém o registro íntegro se o veículo for excluído do cadastro.
        /// </summary>
        public int IdCompany { get; set; }

        /// <summary>Documento que originou o uso (MDF-e ou Ordem de Serviço).</summary>
        public VehicleUsageSource Source { get; set; }

        /// <summary>
        /// Id do documento de origem, quando ele existir no banco. Fica nulo
        /// para MDF-e até que a tabela de manifesto seja criada.
        /// </summary>
        public int? SourceId { get; set; }

        /// <summary>
        /// Referência legível do documento de origem (nº da OS, chave/número do
        /// MDF-e) — preserva o vínculo mesmo sem <see cref="SourceId"/>.
        /// </summary>
        [StringLength(200)]
        public string? Reference { get; set; }

        /// <summary>Quando o veículo foi usado.</summary>
        public DateTime UsedAt { get; set; }

        /// <summary>Quando este registro foi criado (auditoria).</summary>
        public DateTime CreatedAt { get; set; }
    }
}
