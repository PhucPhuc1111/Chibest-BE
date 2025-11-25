using System;
using System.Collections.Generic;

namespace Chibest.Service.ModelDTOs.Stock.FranchiseOrder;

public class FranchiseOrderCreate
{
    public string? InvoiceCode { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal TotalMoney { get; set; }
    public Guid SellerId { get; set; }

    public Guid BranchId { get; set; }

    public ICollection<FranchiseOrderDetailCreate> FranchiseOrderDetails { get; set; } = new List<FranchiseOrderDetailCreate>();
}

public class FranchiseOrderDetailCreate
{
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal CommissionFee { get; set; }

    public string? Note { get; set; }
}

public class FranchiseInvoiceCreate
{
    public string? Code { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;

    public string? Status { get; set; }

    public Guid BranchId { get; set; }

    public ICollection<FranchiseOrderCreate> FranchiseOrders { get; set; } = new List<FranchiseOrderCreate>();
}

