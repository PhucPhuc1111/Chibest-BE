using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public class SupplierDebtHistory
{
    public Guid Id { get; set; }

    public Guid SupplierDebtId { get; set; }

    public string TransactionType { get; set; } = null!;

    public DateTime TransactionDate { get; set; }

    public decimal Amount { get; set; }

    public string? Confirmation { get; set; }

    public string Status { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual SupplierDebt SupplierDebt { get; set; } = null!;
}
