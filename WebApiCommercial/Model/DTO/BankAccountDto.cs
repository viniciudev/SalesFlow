//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Model.DTO
//{
//    internal class BankAccountDto
//    {
//    }
//}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    public class BankAccountDto
    {
        public int Id { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string AgencyNumber { get; set; }
        public string AgencyDigit { get; set; }
        public string AccountNumber { get; set; }
        public string AccountDigit { get; set; }
        public string AccountType { get; set; }
        public string HolderName { get; set; }
        public string HolderDocument { get; set; }
        public string HolderType { get; set; }
        public string PixKey { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateBankAccountDto
    {
        public int idCompany;

        [Required]
        [StringLength(10)]
        public string BankCode { get; set; }

        [Required]
        [StringLength(100)]
        public string BankName { get; set; }

        [Required]
        [StringLength(10)]
        public string AgencyNumber { get; set; }

        [StringLength(5)]
        public string AgencyDigit { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountNumber { get; set; }

        [Required]
        [StringLength(5)]
        public string AccountDigit { get; set; }

        [Required]
        public string AccountType { get; set; }

        [Required]
        [StringLength(200)]
        public string HolderName { get; set; }

        [Required]
        [StringLength(20)]
        public string HolderDocument { get; set; }

        [Required]
        public string HolderType { get; set; }

        [StringLength(100)]
        public string PixKey { get; set; }

        public bool IsActive { get; set; }
    }

    public class UpdateBankAccountDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string BankCode { get; set; }

        [Required]
        [StringLength(100)]
        public string BankName { get; set; }

        [Required]
        [StringLength(10)]
        public string AgencyNumber { get; set; }

        [StringLength(5)]
        public string AgencyDigit { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountNumber { get; set; }

        [Required]
        [StringLength(5)]
        public string AccountDigit { get; set; }

        [Required]
        public string AccountType { get; set; }

        [Required]
        [StringLength(200)]
        public string HolderName { get; set; }

        [Required]
        [StringLength(20)]
        public string HolderDocument { get; set; }

        [Required]
        public string HolderType { get; set; }

        [StringLength(100)]
        public string PixKey { get; set; }

        public bool IsActive { get; set; }
    }

    public class BankAccountFilterDto
    {
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public string BankCode { get; set; }
        public AccountType? AccountType { get; set; }
        public HolderType? HolderType { get; set; }
        public int IdCompany { get; set; }
    }

    
}