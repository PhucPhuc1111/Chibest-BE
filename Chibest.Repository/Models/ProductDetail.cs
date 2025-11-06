using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class ProductDetail
{
    public Guid Id { get; set; }

    public string? BarCode { get; set; }

    public string? ChipCode { get; set; }

    public string? TagId { get; set; }

    public Guid ProductId { get; set; }

    public Guid BranchId { get; set; }

    public Guid? WarehouseId { get; set; }

    public decimal SellingPrice { get; set; }

    public decimal PurchasePrice { get; set; }

    public DateTime ImportDate { get; set; }

    public Guid? SupplierId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? LastTransactionDate { get; set; }

    public string? LastTransactionType { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();

    public virtual Account? Supplier { get; set; }

    public virtual Warehouse? Warehouse { get; set; }
}
