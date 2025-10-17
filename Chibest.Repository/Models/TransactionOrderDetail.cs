using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class TransactionOrderDetail
{
    public Guid Id { get; set; }

    public string ContainerCode { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal? TotalPrice { get; set; }

    public DateTime PurchaseDate { get; set; }

    public string? Note { get; set; }

    public Guid TransactionOrderId { get; set; }

    public Guid ProductId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();

    public virtual TransactionOrder TransactionOrder { get; set; } = null!;
}
