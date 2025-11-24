using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public class ProductPlan
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Guid? SupplierId { get; set; }

    public string? Type { get; set; }

    public DateTime SendDate { get; set; }

    public string? DetailAmount { get; set; }

    public int? Amount { get; set; }

    public string Status { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Account? Supplier { get; set; }
}
