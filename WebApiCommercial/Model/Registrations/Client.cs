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

        [StringLength(60)]
        [EmailAddress]
        public string Email { get; set; }

        public string CellPhone { get; set; }

        [StringLength(60)]
        public string Pais { get; set; } = "Brasil";

        [StringLength(4)]
        public string CodPais { get; set; } = "1058";

        // Campos existentes (mantidos para compatibilidade)
        //[Obsolete("Use NomeRazao instead")]
        //public string Name
        //{
        //    get => NomeRazao;
        //    set => NomeRazao = value;
        //}

        //[Obsolete("Use Document instead")]
        //public string Document
        //{
        //    get => CpfCnpj;
        //    set => CpfCnpj = value;
        //}

        //[Obsolete("Use Address instead")]
        //public string Address
        //{
        //    get => Logradouro;
        //    set => Logradouro = value;
        //}

        //[Obsolete("Use CellPhone instead")]
        //public string CellPhone
        //{
        //    get => Telefone;
        //    set => Telefone = value;
        //}

        //[Obsolete("Use ZipCode instead")]
        //public string ZipCode
        //{
        //    get => Cep;
        //    set => Cep = value;
        //}

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