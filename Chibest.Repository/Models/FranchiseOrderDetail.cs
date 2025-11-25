using System;

namespace Chibest.Repository.Models;

public class FranchiseOrderDetail
{
    public Guid Id { get; set; }

    public int Quantity { get; set; }

    public int? ActualQuantity { get; set; }

    public decimal CommissionFee { get; set; }

    public decimal UnitPrice { get; set; }

    public string? Note { get; set; }

    public Guid ProductId { get; set; }

    public Guid FranchiseOrderId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual FranchiseOrder FranchiseOrder { get; set; } = null!;
}

