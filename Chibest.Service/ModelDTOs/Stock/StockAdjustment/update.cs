using Chibest.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Stock.StockAdjustment
{
    public class update
    {
        public class StockAdjustmentUpdate
        {
            public AdjustmentType? AdjustmentType { get; set; }
            public Guid? ApprovebyId { get; set; }
            public string? Note { get; set; }
            public string? Status { get; set; }
            public List<StockAdjustmentDetailUpdate> StockAdjustmentDetails { get; set; } = new();
        }

        public class StockAdjustmentDetailUpdate
        {
            public Guid Id { get; set; }
            public Guid ProductId { get; set; }
            public int SystemQty { get; set; }
            public int ActualQty { get; set; }
            public decimal UnitCost { get; set; }
            public string? Reason { get; set; }
            public string? Note { get; set; }
        }
    }
}
