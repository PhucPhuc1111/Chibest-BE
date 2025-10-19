using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class PurchaseReturnDetail
{
    public Guid Id { get; set; }

    public string ContainerCode { get; set; } = null!;

    public int Quantity { get; set; }

    public int? ActualQuantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Discount { get; set; }

    public string? Note { get; set; }

    public Guid PurchaseReturnId { get; set; }

    public Guid ProductId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual PurchaseReturn PurchaseReturn { get; set; } = null!;
}
