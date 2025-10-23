using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class AccountRole
{
    public Guid AccountId { get; set; }

    public Guid RoleId { get; set; }

    public Guid? BranchId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Branch? Branch { get; set; }

    public virtual Role Role { get; set; } = null!;
}
