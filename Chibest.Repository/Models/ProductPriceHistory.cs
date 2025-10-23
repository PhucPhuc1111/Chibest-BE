using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class ProductPriceHistory
{
    public Guid Id { get; set; }

    public decimal SellingPrice { get; set; }

    public DateTime EffectiveDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid ProductId { get; set; }

    public Guid? BranchId { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual Product Product { get; set; } = null!;
}
