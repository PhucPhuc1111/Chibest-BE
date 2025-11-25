using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public class FranchiseInvoice
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public decimal TotalMoney { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public Guid BranchId { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<FranchiseOrder> FranchiseOrders { get; set; } = new List<FranchiseOrder>();
}

