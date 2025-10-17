using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class Account
{
    public Guid Id { get; set; }

    public string? FcmToken { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public string? AvartarUrl { get; set; }

    public string Code { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? Cccd { get; set; }

    public string? TaxCode { get; set; }

    public string? FaxNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<Commission> Commissions { get; set; } = new List<Commission>();

    public virtual ICollection<CustomerVoucher> CustomerVouchers { get; set; } = new List<CustomerVoucher>();

    public virtual ICollection<Deduction> Deductions { get; set; } = new List<Deduction>();

    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();

    public virtual ICollection<SalaryConfig> SalaryConfigs { get; set; } = new List<SalaryConfig>();

    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    public virtual ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();

    public virtual ICollection<TransactionOrder> TransactionOrderEmployees { get; set; } = new List<TransactionOrder>();

    public virtual ICollection<TransactionOrder> TransactionOrderSuppliers { get; set; } = new List<TransactionOrder>();
}
