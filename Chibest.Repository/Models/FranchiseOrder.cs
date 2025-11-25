using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public class FranchiseOrder
{
    public Guid Id { get; set; }

    public string InvoiceCode { get; set; } = null!;

    public decimal TotalMoney { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public Guid BranchId { get; set; }

    public Guid? FranchiseInvoiceId { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual FranchiseInvoice? FranchiseInvoice { get; set; }

    public virtual ICollection<FranchiseOrderDetail> FranchiseOrderDetails { get; set; } = new List<FranchiseOrderDetail>();
}

