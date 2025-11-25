using Chibest.Common.Enums;
using System;
using System.Collections.Generic;

namespace Chibest.Service.ModelDTOs.Stock.FranchiseOrder;

public class FranchiseOrderUpdate
{
    public decimal TotalMoney { get; set; }

    public OrderStatus Status { get; set; }

    public ICollection<FranchiseOrderDetailUpdate> FranchiseOrderDetails { get; set; } = new List<FranchiseOrderDetailUpdate>();
}

public class FranchiseOrderDetailUpdate
{
    public Guid Id { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal CommissionFee { get; set; }

    public string? Note { get; set; }

    public int? ActualQuantity { get; set; }
}

