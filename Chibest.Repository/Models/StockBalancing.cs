using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class StockBalancing
{
    public Guid Id { get; set; }

    public string BalancingCode { get; set; } = null!;

    public Guid StockAdjustmentId { get; set; }

    public Guid BranchId { get; set; }

    public Guid? WarehouseId { get; set; }

    public Guid EmployeeId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public decimal? TotalPositiveAdjust { get; set; }

    public decimal? TotalNegativeAdjust { get; set; }

    public decimal? NetValueChange { get; set; }

    public string Status { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public virtual Account? ApprovedByNavigation { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Account Employee { get; set; } = null!;

    public virtual StockAdjustment StockAdjustment { get; set; } = null!;

    public virtual ICollection<StockBalancingDetail> StockBalancingDetails { get; set; } = new List<StockBalancingDetail>();

    public virtual Warehouse? Warehouse { get; set; }
}
