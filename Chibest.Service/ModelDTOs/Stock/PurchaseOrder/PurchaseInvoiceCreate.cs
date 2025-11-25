using System;
using System.Collections.Generic;

namespace Chibest.Service.ModelDTOs.Stock.PurchaseOrder;

public class PurchaseInvoiceCreate
{
    public string? Code { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;

    public string? Status { get; set; }

    public Guid SupplierId { get; set; }

    public ICollection<PurchaseOrderCreate> PurchaseOrders { get; set; } = new List<PurchaseOrderCreate>();
}

