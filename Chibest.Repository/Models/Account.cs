using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public class Account
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public string? FcmToken { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<Commission> Commissions { get; set; } = new List<Commission>();

    public virtual ICollection<Deduction> Deductions { get; set; } = new List<Deduction>();

    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();

    public virtual ICollection<ProductPlan> ProductPlans { get; set; } = new List<ProductPlan>();

    public virtual ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();

    public virtual ICollection<PurchaseOrder> PurchaseOrderEmployees { get; set; } = new List<PurchaseOrder>();

    public virtual ICollection<PurchaseOrder> PurchaseOrderSuppliers { get; set; } = new List<PurchaseOrder>();

    public virtual ICollection<PurchaseReturn> PurchaseReturnEmployees { get; set; } = new List<PurchaseReturn>();

    public virtual ICollection<PurchaseReturn> PurchaseReturnSuppliers { get; set; } = new List<PurchaseReturn>();

    public virtual ICollection<SalaryConfig> SalaryConfigs { get; set; } = new List<SalaryConfig>();

    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    public virtual ICollection<StockAdjustment> StockAdjustmentApprovedByNavigations { get; set; } = new List<StockAdjustment>();

    public virtual ICollection<StockAdjustment> StockAdjustmentEmployees { get; set; } = new List<StockAdjustment>();

    public virtual SupplierDebt? SupplierDebt { get; set; }

    public virtual ICollection<TransferOrder> TransferOrders { get; set; } = new List<TransferOrder>();
}
