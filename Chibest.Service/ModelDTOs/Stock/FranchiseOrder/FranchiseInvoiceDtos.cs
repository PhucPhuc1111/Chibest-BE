using Chibest.Common.Enums;
using System;
using System.Collections.Generic;

namespace Chibest.Service.ModelDTOs.Stock.FranchiseOrder;

public class FranchiseInvoiceList
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public Guid BranchId { get; set; }

    public string? BranchName { get; set; }

    public decimal TotalMoney { get; set; }
}

public class FranchiseInvoiceResponse
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public Guid BranchId { get; set; }

    public string? BranchName { get; set; }

    public decimal TotalMoney { get; set; }

    public ICollection<FranchiseOrderResponse> FranchiseOrders { get; set; } = new List<FranchiseOrderResponse>();
}

public class FranchiseInvoiceStatusUpdate
{
    public OrderStatus Status { get; set; }
}

