using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class Product
{
    public Guid Id { get; set; }

    public string Sku { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Color { get; set; }

    public string? Size { get; set; }

    public string? Style { get; set; }

    public string? Brand { get; set; }

    public string? Material { get; set; }

    public int Weight { get; set; }

    public bool IsMaster { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid CategoryId { get; set; }

    public string? ParentSku { get; set; }

    public virtual ICollection<BranchStock> BranchStocks { get; set; } = new List<BranchStock>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Product> InverseParentSkuNavigation { get; set; } = new List<Product>();

    public virtual Product? ParentSkuNavigation { get; set; }

    public virtual ICollection<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();

    public virtual ICollection<ProductPriceHistory> ProductPriceHistories { get; set; } = new List<ProductPriceHistory>();

    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();

    public virtual ICollection<PurchaseReturnDetail> PurchaseReturnDetails { get; set; } = new List<PurchaseReturnDetail>();

    public virtual ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();

    public virtual ICollection<StockAdjustmentDetail> StockAdjustmentDetails { get; set; } = new List<StockAdjustmentDetail>();

    public virtual ICollection<TransferOrderDetail> TransferOrderDetails { get; set; } = new List<TransferOrderDetail>();
}
