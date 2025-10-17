using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class Commission
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public Guid? SalesOrderId { get; set; }

    public string CommissionType { get; set; } = null!;

    public decimal Amount { get; set; }

    public decimal? CalculationBase { get; set; }

    public decimal? CommissionRate { get; set; }

    public int PeriodMonth { get; set; }

    public int PeriodYear { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account Employee { get; set; } = null!;

    public virtual SalesOrder? SalesOrder { get; set; }
}
