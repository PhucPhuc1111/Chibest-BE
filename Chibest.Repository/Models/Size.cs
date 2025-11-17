using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class Size
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
