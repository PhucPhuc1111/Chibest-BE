using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public class Deduction
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public string DeductionType { get; set; } = null!;

    public decimal Amount { get; set; }

    public int PeriodMonth { get; set; }

    public int PeriodYear { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account Employee { get; set; } = null!;
}
