using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class MonthlySalesComparisonResult
    {
        public MonthData CurrentMonth { get; set; }
        public MonthData PreviousMonth { get; set; }
        public decimal PercentageChange { get; set; }
        public bool IsIncrease { get; set; }

        public class MonthData
        {
            public int Month { get; set; }
            public int Year { get; set; }
            public decimal TotalSales { get; set; }
            public int SalesCount { get; set; }
        }
    }
}
