using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class StockBalancingDetail
{
    public Guid Id { get; set; }

    public Guid StockBalancingId { get; set; }

    public Guid ProductId { get; set; }

    public int SystemQtyAtStart { get; set; }

    public int SystemQtyAfterSales { get; set; }

    public int ActualQty { get; set; }

    public int FinalQty { get; set; }

    public int? DifferenceQty { get; set; }

    public decimal UnitCost { get; set; }

    public decimal? ValueChange { get; set; }

    public bool IsBalanced { get; set; }

    public string? Note { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual StockBalancing StockBalancing { get; set; } = null!;
}
