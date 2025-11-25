using System;

namespace Chibest.Service.ModelDTOs.Stock.FranchiseOrder;

public class FranchiseOrderList
{
    public Guid Id { get; set; }

    public string InvoiceCode { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public string? BranchName { get; set; }

    public decimal TotalMoney { get; set; }

    public string Status { get; set; } = null!;
}

