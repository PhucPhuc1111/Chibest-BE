using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class TransferOrder
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

    public Guid? FromBranch { get; set; }

    public Guid? ToBranch { get; set; }

    public virtual Account? Employee { get; set; }

    public virtual Branch? FromBranchNavigation { get; set; }

    public virtual Branch? ToBranchNavigation { get; set; }

    public virtual ICollection<TransferOrderDetail> TransferOrderDetails { get; set; } = new List<TransferOrderDetail>();
}
