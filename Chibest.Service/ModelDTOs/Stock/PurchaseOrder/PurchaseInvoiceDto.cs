using Chibest.Common.Enums;
using System;
using System.Collections.Generic;

namespace Chibest.Service.ModelDTOs.Stock.PurchaseOrder;

public class PurchaseInvoiceList
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public Guid SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public decimal TotalMoney { get; set; }
}

public class PurchaseInvoiceResponse
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public Guid SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public decimal TotalMoney { get; set; }

    public ICollection<PurchaseOrderResponse> PurchaseOrders { get; set; } = new List<PurchaseOrderResponse>();
}

public class PurchaseInvoiceStatusUpdate
{
    public OrderStatus Status { get; set; }
}

