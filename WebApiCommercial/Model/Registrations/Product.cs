﻿using Model.Moves;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Registrations
{
    public class Product : BaseEntity
    {
        public int IdCompany { get; set; }
        public Company Company { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public decimal Quantity { get; set; }
        public string Description { get; set; }
        public byte[] ?Image { get; set; }
        public ICollection<SaleItems> SaleItems { get; set; }
        public ICollection<Commission> Commissions { get; set; }
        public ICollection<Financial> Financials { get; set; }
        [NotMapped]
        public string ImageBytes { get; set; }
    }
}
