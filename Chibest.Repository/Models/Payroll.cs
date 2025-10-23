using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class Payroll
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public Guid BranchId { get; set; }

    public int PeriodMonth { get; set; }

    public int PeriodYear { get; set; }

    public int TotalWorkDays { get; set; }

    public decimal TotalWorkHours { get; set; }

    public decimal TotalOvertimeHours { get; set; }

    public int StandardWorkDays { get; set; }

    public decimal BaseSalary { get; set; }

    public decimal ActualBaseSalary { get; set; }

    public decimal TotalAllowance { get; set; }

    public decimal OvertimeSalary { get; set; }

    public decimal TotalCommission { get; set; }

    public decimal TotalBonus { get; set; }

    public decimal TotalDeduction { get; set; }

    public decimal SocialInsurance { get; set; }

    public decimal HealthInsurance { get; set; }

    public decimal UnemploymentInsurance { get; set; }

    public decimal TaxableIncome { get; set; }

    public decimal PersonalTax { get; set; }

    public decimal? GrossSalary { get; set; }

    public decimal? NetSalary { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public string? PaymentMethod { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Account Employee { get; set; } = null!;
}
