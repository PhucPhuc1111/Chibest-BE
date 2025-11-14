using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class StockAdjustmentDetail
{
    public Guid Id { get; set; }

    public Guid StockAdjustmentId { get; set; }

    public Guid ProductId { get; set; }

    public int SystemQty { get; set; }

    public int ActualQty { get; set; }

    public int? DifferenceQty { get; set; }

    public decimal UnitCost { get; set; }

    public decimal? TotalValueChange { get; set; }

    public string? Note { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual StockAdjustment StockAdjustment { get; set; } = null!;
}
