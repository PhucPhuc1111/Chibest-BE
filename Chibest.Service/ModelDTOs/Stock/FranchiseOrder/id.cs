using System;
using System.Collections.Generic;

namespace Chibest.Service.ModelDTOs.Stock.FranchiseOrder;

public class FranchiseOrderResponse
{
    public Guid Id { get; set; }

    public string InvoiceCode { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public decimal TotalMoney { get; set; }

    public string Status { get; set; } = null!;

    public string? BranchName { get; set; }

    public ICollection<FranchiseOrderDetailResponse> FranchiseOrderDetails { get; set; } = new List<FranchiseOrderDetailResponse>();
}

public class FranchiseOrderDetailResponse
{
    public Guid Id { get; set; }

    public int Quantity { get; set; }

    public int? ActualQuantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal CommissionFee { get; set; }

    public string? Note { get; set; }

    public string? ProductName { get; set; }

    public string Sku { get; set; } = null!;
}

