using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class MonthlyClientsComparisonResult
    {
        public MonthData CurrentMonth { get; set; }
        public MonthData PreviousMonth { get; set; }
        public decimal PercentageChange { get; set; }
        public bool IsIncrease { get; set; }
        public int TotalComparison { get; set; } // Diferença absoluta

        public class MonthData
        {
            public int Month { get; set; }
            public int Year { get; set; }
            public int ClientCount { get; set; }
        }
    }
}
