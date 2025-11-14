using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class BranchDebtHistory
{
    public Guid Id { get; set; }

    public Guid BranchDebtId { get; set; }

    public string TransactionType { get; set; } = null!;

    public DateTime TransactionDate { get; set; }

    public decimal Amount { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual BranchDebt BranchDebt { get; set; } = null!;
}
