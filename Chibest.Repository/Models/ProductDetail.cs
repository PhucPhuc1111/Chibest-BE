using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public class ProductDetail
{
    public Guid Id { get; set; }

    public string? ChipCode { get; set; }

    public string? TagId { get; set; }

    public Guid ProductId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();
}
