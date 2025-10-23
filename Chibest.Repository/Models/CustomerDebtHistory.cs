using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class CustomerDebtHistory
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public string TransactionType { get; set; } = null!;

    public DateTime TransactionDate { get; set; }

    public decimal Amount { get; set; }

    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }

    public string? ReferenceCode { get; set; }

    public decimal BalanceBefore { get; set; }

    public decimal BalanceAfter { get; set; }

    public string? Note { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
