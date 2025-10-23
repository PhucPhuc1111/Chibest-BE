using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class SalesReturn
{
    public Guid Id { get; set; }

    public string ReturnCode { get; set; } = null!;

    public DateTime ReturnDate { get; set; }

    public Guid? SalesOrderId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid BranchId { get; set; }

    public Guid? EmployeeId { get; set; }

    public decimal RefundAmount { get; set; }

    public string RefundMethod { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? Reason { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Account? Employee { get; set; }

    public virtual SalesOrder? SalesOrder { get; set; }

    public virtual ICollection<SalesReturnDetail> SalesReturnDetails { get; set; } = new List<SalesReturnDetail>();
}
