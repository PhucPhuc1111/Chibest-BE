using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class SupplierDebt
{
    public Guid Id { get; set; }

    public Guid SupplierId { get; set; }

    public decimal TotalDebt { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual Account Supplier { get; set; } = null!;
}
