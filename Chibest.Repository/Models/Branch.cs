using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class Branch
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string Status { get; set; } = null!;

    public bool IsFranchise { get; set; }

    public string? OwnerName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual BranchDebt? BranchDebt { get; set; }

    public virtual ICollection<BranchDebtHistory> BranchDebtHistories { get; set; } = new List<BranchDebtHistory>();

    public virtual ICollection<BranchStock> BranchStocks { get; set; } = new List<BranchStock>();

    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();

    public virtual ICollection<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();

    public virtual ICollection<ProductPriceHistory> ProductPriceHistories { get; set; } = new List<ProductPriceHistory>();

    public virtual ICollection<SalaryConfig> SalaryConfigs { get; set; } = new List<SalaryConfig>();

    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    public virtual ICollection<SalesReturn> SalesReturns { get; set; } = new List<SalesReturn>();

    public virtual ICollection<StockAdjustment> StockAdjustments { get; set; } = new List<StockAdjustment>();

    public virtual ICollection<StockBalancing> StockBalancings { get; set; } = new List<StockBalancing>();

    public virtual ICollection<StockMovement> StockMovementFromBranches { get; set; } = new List<StockMovement>();

    public virtual ICollection<StockMovement> StockMovementToBranches { get; set; } = new List<StockMovement>();

    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
}
