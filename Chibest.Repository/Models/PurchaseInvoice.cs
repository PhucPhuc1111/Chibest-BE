using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public class PurchaseInvoice
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public decimal TotalMoney { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public Guid SupplierId { get; set; }

    public virtual Account Supplier { get; set; } = null!;

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}

