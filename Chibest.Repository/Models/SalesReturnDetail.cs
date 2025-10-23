using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class SalesReturnDetail
{
    public Guid Id { get; set; }

    public Guid SalesReturnId { get; set; }

    public Guid? SalesOrderDetailId { get; set; }

    public Guid ProductId { get; set; }

    public Guid? ProductDetailId { get; set; }

    public int ReturnQty { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal RefundAmount { get; set; }

    public string Condition { get; set; } = null!;

    public string? Note { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ProductDetail? ProductDetail { get; set; }

    public virtual SalesOrderDetail? SalesOrderDetail { get; set; }

    public virtual SalesReturn SalesReturn { get; set; } = null!;
}
