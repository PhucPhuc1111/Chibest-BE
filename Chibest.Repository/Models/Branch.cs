using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class Branch
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string Status { get; set; } = null!;

    public bool IsFranchise { get; set; }

    public string? OwnerName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();

    public virtual BranchDebt? BranchDebt { get; set; }

    public virtual ICollection<BranchDebtHistory> BranchDebtHistories { get; set; } = new List<BranchDebtHistory>();

    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
}
