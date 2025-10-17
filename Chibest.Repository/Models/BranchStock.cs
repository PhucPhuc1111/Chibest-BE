using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class BranchStock
{
    public Guid Id { get; set; }

    public decimal SellingPrice { get; set; }

    public int ReOrderPriority { get; set; }

    public int AvailableQty { get; set; }

    public int ReservedQty { get; set; }

    public int MinimumStock { get; set; }

    public DateTime LastUpdated { get; set; }

    public Guid ProductId { get; set; }

    public Guid BranchId { get; set; }

    public virtual Warehouse Branch { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
