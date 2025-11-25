using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public class PurchaseOrder
{
    public Guid Id { get; set; }

    public string InvoiceCode { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public decimal SubTotal { get; set; }

    public string? Note { get; set; }

    public string Status { get; set; } = null!;

    public Guid? BranchId { get; set; }

    public Guid? EmployeeId { get; set; }

    public Guid? SupplierId { get; set; }

    public Guid? PurchaseInvoiceId { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Account? Employee { get; set; }

    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();

    public virtual Account? Supplier { get; set; }

    public virtual PurchaseInvoice? PurchaseInvoice { get; set; }
}
