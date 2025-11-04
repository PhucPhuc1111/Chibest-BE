using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class BranchDebt
{
    public Guid Id { get; set; }

    public Guid BranchId { get; set; }

    public decimal TotalDebt { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal ReturnAmount { get; set; }

    public decimal? RemainingDebt { get; set; }

    public DateTime? LastTransactionDate { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<BranchDebtHistory> BranchDebtHistories { get; set; } = new List<BranchDebtHistory>();
}
