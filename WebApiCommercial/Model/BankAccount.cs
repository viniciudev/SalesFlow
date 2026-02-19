
using Model.Moves;
using Model.Registrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class BankAccount:BaseEntity
    {
        public Company Company { get; set; }
        public int IdCompany { get; set; }
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
        public AccountType AccountType { get; set; }

        [Required]
        [StringLength(200)]
        public string HolderName { get; set; }

        [Required]
        [StringLength(20)]
        public string HolderDocument { get; set; }

        [Required]
        public HolderType HolderType { get; set; }

        [StringLength(100)]
        public string PixKey { get; set; }

        [Required]
        public bool IsActive { get; set; }
        public ICollection<Financial> Financials { get; set; }
    }


    public enum AccountType
    {
        checking,
        savings,
        salary
    }

    public enum HolderType
    {
        individual,
        company
    }
}
