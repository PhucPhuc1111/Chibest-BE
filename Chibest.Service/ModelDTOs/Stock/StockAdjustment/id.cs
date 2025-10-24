using Chibest.Common.Enums;
using Chibest.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Chibest.Service.ModelDTOs.Stock.PurchaseReturn.id;

namespace Chibest.Service.ModelDTOs.Stock.StockAdjustment
{
    public class id
    {
        public class StockAdjustmentResponse
        {

            public Guid Id { get; set; }

            public string AdjustmentCode { get; set; } = null!;

            public DateTime AdjustmentDate { get; set; }

            public string? AdjustmentType { get; set; }

            public string? BranchName { get; set; }

            public string? WarehouseName { get; set; }

            public string? EmployeeName { get; set; }

            public string? ApproveName { get; set; }

            public decimal TotalValueChange { get; set; }

            public string Status { get; set; } = null!;

            public string? Reason { get; set; }

            public string? Note { get; set; }

            public DateTime CreatedAt { get; set; }

            public DateTime UpdatedAt { get; set; }
            public DateTime? ApprovedAt { get; set; }
            public virtual ICollection<StockAdjustmentDetailResponse> StockAdjustmentDetails { get; set; } = new List<StockAdjustmentDetailResponse>();
        }
        public class StockAdjustmentDetailResponse
        {
            public Guid Id { get; set; }

            public int SystemQty { get; set; }

            public int ActualQty { get; set; }

            public int? DifferenceQty { get; set; }

            public decimal UnitCost { get; set; }

            public decimal? TotalValueChange { get; set; }

            public string? Reason { get; set; }

            public string? Note { get; set; }

            public string? ProductName { get; set; }

            public string Sku { get; set; } = null!;

        }
    }
}