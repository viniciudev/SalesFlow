//using Model.Moves;
//using Model.Registrations;
//using System;
//using System.Collections.Generic;

//namespace Model
//{
//    public class Client : BaseEntity
//    {
//        public int IdCompany { get; set; }
//        public Company Company { get; set; }
//        public string Document { get; set; }
//        [Uppercase]
//        public string Name { get; set; }
//        public string Email { get; set; }
//        public string CellPhone { get; set; }
//        public string ZipCode { get; set; }
//        [Uppercase]
//        public string Address { get; set; }
//        public string Bairro { get; set; }
//        public string Complement { get; set; }
//        public string NameState { get; set; }
//        public string NameCity { get; set; }
//        public DateTime BirthDate { get; set; }
//        public DateTime CreatDate { get; set; }
//        public statusType Status { get; set; }
//        public ICollection<ServicesProvision> ServiceProvisions { get; set; }
//        public ICollection<Sale> Sale { get; set; }
//        public ICollection<Financial> Financials { get; set; }
//        public string Numero { get; set; }
//        public long CodigoMunicipio { get; set; }

//        public enum statusType
//        {
//            Ativo,
//            Inativo,
//        }
//    }

//}
// Model/Client.cs
using Model.Enums;
using Model.Moves;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class Client : BaseEntity
    {
        public int IdCompany { get; set; }
        public Company Company { get; set; }

        // Campos Obrigatórios
        [Required]
        [StringLength(1)]
        public string TipoPessoa { get; set; } // 'F' = Física, 'J' = Jurídica

        [Required]
        [StringLength(60)]
        [Uppercase]
        public string Name { get; set; }

        [Required]
       
        public string Document { get; set; } // somente números

        [Required]
        [StringLength(1)]
        public string IndicadorIE { get; set; } // '1'=Contribuinte, '2'=Isento, '9'=Não contribuinte

        [Required]
        [StringLength(1)]
        public string ConsumidorFinal { get; set; } // '1' = Sim

        [Required]
        [StringLength(60)]
        public string Address { get; set; }

        [Required]
        [StringLength(60)]
        public string Numero { get; set; }

        [Required]
        [StringLength(60)]
        public string Bairro { get; set; }

        [Required]
        [StringLength(60)]
        public string Municipio { get; set; }

        [Required]
        [StringLength(7)]
        public string CodMunicipioIbge { get; set; }

        [Required]
        [StringLength(2)]
        public string Uf { get; set; }

        [Required]
        [StringLength(8)]
        public string ZipCode { get; set; }

        // Campos Necessários
        [StringLength(60)]
        public string Complemento { get; set; }

        [StringLength(14)]
        public string Ie { get; set; } // Obrigatório se indicadorIE = "1"

        [StringLength(20)]
        public string InscricaoMunicipal { get; set; }

        /// <summary>
        /// RNTRC — Registro Nacional de Transportador de Cargas (ANTT).
        ///
        /// Só faz sentido para o parceiro com perfil Transportadora (ou o
        /// proprietário terceiro de um veículo), por isso é nulo para a maioria
        /// dos registros. Passou a existir para atender à RV10 do cadastro de
        /// veículos: o MDF-e exige o RNTRC do proprietário quando o veículo não
        /// é próprio (<c>veicReboque/RNTRC</c> no XSD
        /// <c>mdfeModalRodoviario_v3.00.xsd</c>).
        ///
        /// São SEMPRE 8 dígitos — não há dígito verificador nem variação de
        /// formato. É o tipo <c>TRNTRC</c> dos XSDs do MDF-e, cujo
        /// <c>&lt;xs:pattern value="[0-9]{8}"/&gt;</c> é idêntico nas versões
        /// 1.00 e 3.00. O limite aqui tem de acompanhar a coluna
        /// (<c>varchar(8)</c>, ver <c>ContextBase.ConfiguraClient</c>): com um
        /// limite maior o valor passaria pelo <c>ModelState</c> e só estouraria
        /// no Postgres (22001), virando erro 500 em vez de mensagem de validação.
        /// </summary>
        [StringLength(8)]
        public string? Rntrc { get; set; }

        [StringLength(60)]
   
        public string? Email { get; set; }

        public string CellPhone { get; set; }

        [StringLength(60)]
        public string Pais { get; set; } = "Brasil";

        [StringLength(4)]
        public string CodPais { get; set; } = "1058";
        [Obsolete("Use NameCity instead")]
        public string NameCity
        {
            get ;
            set ;
        }

        [Obsolete("Use NameState instead")]
        public string NameState
        {
            get ;
            set ;
        }

        /// <summary>
        /// Classificações do parceiro (Cliente, Fornecedor, Transportadora,
        /// Motorista, Outros) — multi-seleção via enum [Flags], persistido como
        /// integer em tb_client.Profiles.
        ///
        /// NOTA: "Fornecedor" aqui é uma classificação do parceiro comercial.
        /// Já existe a entidade <see cref="Provider"/> com o mesmo sentido no
        /// fluxo de compras — são fontes de verdade sobrepostas, mantidas
        /// separadas de propósito. Unificar é decisão de produto, não técnica.
        /// </summary>
        public PartnerProfile Profiles { get; set; } = PartnerProfile.Cliente;

        /// <summary>
        /// Dados de CNH — preenchidos apenas quando Profiles inclui Motorista.
        /// </summary>
        public DriverLicense? DriverLicense { get; set; }

        // Relacionamentos
        public DateTime? BirthDate { get; set; }
        public DateTime CreatDate { get; set; }
        public statusType Status { get; set; }

        public ICollection<ServicesProvision> ServiceProvisions { get; set; }
        public ICollection<Sale> Sale { get; set; }
        public ICollection<Financial> Financials { get; set; }

        public enum statusType
        {
            Ativo,
            Inativo,
        }
    }
}