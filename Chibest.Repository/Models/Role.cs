using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public class Role
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
