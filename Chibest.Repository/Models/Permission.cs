using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public class Permission
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
