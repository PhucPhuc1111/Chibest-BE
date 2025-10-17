using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class SalaryConfig
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public Guid BranchId { get; set; }

    public string SalaryType { get; set; } = null!;

    public decimal BaseSalary { get; set; }

    public decimal? HourlyRate { get; set; }

    public decimal PositionAllowance { get; set; }

    public decimal TransportAllowance { get; set; }

    public decimal MealAllowance { get; set; }

    public decimal PhoneAllowance { get; set; }

    public decimal HousingAllowance { get; set; }

    public decimal OvertimeCoefficient { get; set; }

    public decimal HolidayCoefficient { get; set; }

    public decimal WeekendCoefficient { get; set; }

    public DateOnly EffectiveDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Warehouse Branch { get; set; } = null!;

    public virtual Account Employee { get; set; } = null!;
}
