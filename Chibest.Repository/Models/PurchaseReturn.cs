using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public class PurchaseReturn
{
    public Guid Id { get; set; }

    public string InvoiceCode { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public decimal SubTotal { get; set; }

    public string? Note { get; set; }

    public string Status { get; set; } = null!;

    public Guid? EmployeeId { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? SupplierId { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Account? Employee { get; set; }

    public virtual ICollection<PurchaseReturnDetail> PurchaseReturnDetails { get; set; } = new List<PurchaseReturnDetail>();

    public virtual Account? Supplier { get; set; }
}
