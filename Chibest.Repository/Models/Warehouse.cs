using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class Warehouse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? FaxNumber { get; set; }

    public bool IsMainWarehouse { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? ParentWarehouseId { get; set; }

    public virtual ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<BranchStock> BranchStocks { get; set; } = new List<BranchStock>();

    public virtual ICollection<Warehouse> InverseParentWarehouse { get; set; } = new List<Warehouse>();

    public virtual Warehouse? ParentWarehouse { get; set; }

    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();

    public virtual ICollection<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();

    public virtual ICollection<SalaryConfig> SalaryConfigs { get; set; } = new List<SalaryConfig>();

    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    public virtual ICollection<TransactionOrder> TransactionOrderFromWarehouses { get; set; } = new List<TransactionOrder>();

    public virtual ICollection<TransactionOrder> TransactionOrderToWarehouses { get; set; } = new List<TransactionOrder>();
}
