using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class Warehouse
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public bool IsMainWarehouse { get; set; }

    public bool IsOnlineWarehouse { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? BranchId { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<BranchStock> BranchStocks { get; set; } = new List<BranchStock>();

    public virtual ICollection<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    public virtual ICollection<PurchaseReturn> PurchaseReturns { get; set; } = new List<PurchaseReturn>();

    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    public virtual ICollection<StockAdjustment> StockAdjustments { get; set; } = new List<StockAdjustment>();

    public virtual ICollection<TransferOrder> TransferOrderFromWarehouses { get; set; } = new List<TransferOrder>();

    public virtual ICollection<TransferOrder> TransferOrderToWarehouses { get; set; } = new List<TransferOrder>();
}
