using Chibest.Common.Enums;
using Chibest.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Stock.StockAdjustment
{
    public class create { 
        public class StockAdjustmentCreate
        {
            public string? AdjustmentCode { get; set; }
            public DateTime AdjustmentDate { get; set; }
            public AdjustmentType AdjustmentType { get; set; }
            public Guid BranchId { get; set; }
            public Guid? WarehouseId { get; set; }
            public Guid EmployeeId { get; set; }
            public string? Status { get; set; }
            public string? Note { get; set; }
            public virtual ICollection<StockAdjustmentDetailCreate> StockAdjustmentDetails { get; set; } = new List<StockAdjustmentDetailCreate>();

        }
        public class StockAdjustmentDetailCreate
        {
            public Guid ProductId { get; set; }
            public int SystemQty { get; set; }

            public int ActualQty { get; set; }


            public decimal UnitCost { get; set; }

        }
    }
}
