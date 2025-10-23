using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class PurchasePriceHistory
{
    public Guid Id { get; set; }

    public decimal PurchasePrice { get; set; }

    public DateTime EffectiveDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public int? MinOrderQty { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid ProductId { get; set; }

    public Guid? SupplierId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Account? Supplier { get; set; }
}
