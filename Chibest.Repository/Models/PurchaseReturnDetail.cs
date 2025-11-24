using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public class PurchaseReturnDetail
{
    public Guid Id { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public string? Note { get; set; }

    public Guid PurchaseReturnId { get; set; }

    public Guid ProductId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual PurchaseReturn PurchaseReturn { get; set; } = null!;
}
