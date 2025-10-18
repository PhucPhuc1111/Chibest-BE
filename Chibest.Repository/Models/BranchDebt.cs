using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class BranchDebt
{
    public Guid Id { get; set; }

    public Guid BranchId { get; set; }

    public decimal TotalDebt { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual Branch Branch { get; set; } = null!;
}
