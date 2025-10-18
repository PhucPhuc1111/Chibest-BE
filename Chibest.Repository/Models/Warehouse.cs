using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class Warehouse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public bool IsMainWarehouse { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? BranchId { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<BranchStock> BranchStockOnlineWarehouses { get; set; } = new List<BranchStock>();

    public virtual ICollection<BranchStock> BranchStockWarehouses { get; set; } = new List<BranchStock>();

    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();

    public virtual ICollection<ProductDetail> ProductDetailOnlineWarehouses { get; set; } = new List<ProductDetail>();

    public virtual ICollection<ProductDetail> ProductDetailWarehouses { get; set; } = new List<ProductDetail>();

    public virtual ICollection<SalaryConfig> SalaryConfigs { get; set; } = new List<SalaryConfig>();

    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    public virtual ICollection<TransactionOrder> TransactionOrderFromWarehouses { get; set; } = new List<TransactionOrder>();

    public virtual ICollection<TransactionOrder> TransactionOrderToWarehouses { get; set; } = new List<TransactionOrder>();
}
