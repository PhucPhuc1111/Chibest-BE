using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class PayrollDetail
{
    public Guid Id { get; set; }

    public Guid PayrollId { get; set; }

    public string ItemType { get; set; } = null!;

    public string ItemName { get; set; } = null!;

    public decimal ItemAmount { get; set; }

    public bool IsAddition { get; set; }

    public string? Description { get; set; }

    public Guid? ReferenceId { get; set; }

    public virtual Payroll Payroll { get; set; } = null!;
}
