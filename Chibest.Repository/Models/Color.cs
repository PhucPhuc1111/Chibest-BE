using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class Color
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
