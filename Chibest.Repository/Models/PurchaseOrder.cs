using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class PurchaseOrder
{
    public Guid Id { get; set; }

    public string InvoiceCode { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? PayMethod { get; set; }

    public decimal SubTotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal Paid { get; set; }

    public string? Note { get; set; }

    public string Status { get; set; } = null!;

    public Guid? WarehouseId { get; set; }

    public Guid? EmployeeId { get; set; }

    public Guid? SupplierId { get; set; }

    public virtual Account? Employee { get; set; }

    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();

    public virtual Account? Supplier { get; set; }

    public virtual Warehouse? Warehouse { get; set; }
}
