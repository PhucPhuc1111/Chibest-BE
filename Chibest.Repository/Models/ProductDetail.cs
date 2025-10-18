using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class ProductDetail
{
    public Guid Id { get; set; }

    public string? ChipCode { get; set; }

    public decimal PurchasePrice { get; set; }

    public DateTime ImportDate { get; set; }

    public DateTime? LastTransactionDate { get; set; }

    public string? Status { get; set; }

    public Guid ProductId { get; set; }

    public Guid BranchId { get; set; }

    public Guid? WarehouseId { get; set; }

    public Guid? OnlineWarehouseId { get; set; }

    public string? ContainerCode { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual TransactionOrderDetail? ContainerCodeNavigation { get; set; }

    public virtual Warehouse? OnlineWarehouse { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();

    public virtual Warehouse? Warehouse { get; set; }
}
