using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class StockAdjustment
{
    public Guid Id { get; set; }

    public string AdjustmentCode { get; set; } = null!;

    public DateTime AdjustmentDate { get; set; }

    public string AdjustmentType { get; set; } = null!;

    public Guid BranchId { get; set; }

    public Guid? WarehouseId { get; set; }

    public Guid EmployeeId { get; set; }

    public decimal TotalValueChange { get; set; }

    public string Status { get; set; } = null!;

    public string? Reason { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public virtual Account? ApprovedByNavigation { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Account Employee { get; set; } = null!;

    public virtual ICollection<StockAdjustmentDetail> StockAdjustmentDetails { get; set; } = new List<StockAdjustmentDetail>();

    public virtual Warehouse? Warehouse { get; set; }
}
