using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class SupplierDebt
{
    public Guid Id { get; set; }

    public Guid SupplierId { get; set; }

    public decimal TotalDebt { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal ReturnAmount { get; set; }

    public decimal? RemainingDebt { get; set; }

    public virtual Account Supplier { get; set; } = null!;

    public virtual ICollection<SupplierDebtHistory> SupplierDebtHistories { get; set; } = new List<SupplierDebtHistory>();
}
