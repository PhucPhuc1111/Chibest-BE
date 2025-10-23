using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class StockMovement
{
    public Guid Id { get; set; }

    public string MovementCode { get; set; } = null!;

    public string MovementType { get; set; } = null!;

    public DateTime MovementDate { get; set; }

    public Guid ProductId { get; set; }

    public Guid? FromBranchId { get; set; }

    public Guid? FromWarehouseId { get; set; }

    public Guid? ToBranchId { get; set; }

    public Guid? ToWarehouseId { get; set; }

    public int Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? TotalValue { get; set; }

    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }

    public string? ReferenceCode { get; set; }

    public string? ProductDetailIds { get; set; }

    public string? Note { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual Branch? FromBranch { get; set; }

    public virtual Warehouse? FromWarehouse { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Branch? ToBranch { get; set; }

    public virtual Warehouse? ToWarehouse { get; set; }
}
