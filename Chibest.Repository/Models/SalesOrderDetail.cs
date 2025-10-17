using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class SalesOrderDetail
{
    public Guid Id { get; set; }

    public string ItemName { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? Note { get; set; }

    public Guid SalesOrderId { get; set; }

    public Guid ProductId { get; set; }

    public string? ProductSku { get; set; }

    public Guid? ProductDetailId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ProductDetail? ProductDetail { get; set; }

    public virtual Product? ProductSkuNavigation { get; set; }

    public virtual SalesOrder SalesOrder { get; set; } = null!;
}
