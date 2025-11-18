using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class BranchStock
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Guid BranchId { get; set; }

    public int AvailableQty { get; set; }

    public int MinimumStock { get; set; }

    public int MaximumStock { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
