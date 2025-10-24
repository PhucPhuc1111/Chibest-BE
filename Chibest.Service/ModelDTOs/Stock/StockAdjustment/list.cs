using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Stock.StockAdjustment
{
    public class list
    {
        public class StockAdjustmentList
        {
            public Guid Id { get; set; }

            public string AdjustmentCode { get; set; } = null!;

            public DateTime AdjustmentDate { get; set; }

            public string AdjustmentType { get; set; } = null!;

            public decimal TotalValueChange { get; set; }

            public string Status { get; set; } = null!;
        }
    }
}
