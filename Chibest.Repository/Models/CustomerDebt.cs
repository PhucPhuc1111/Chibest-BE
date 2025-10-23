using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class CustomerDebt
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public decimal TotalDebt { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal? RemainingDebt { get; set; }

    public DateTime? LastTransactionDate { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
