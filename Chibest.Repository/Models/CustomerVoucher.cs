using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class CustomerVoucher
{
    public DateTime CollectedDate { get; set; }

    public DateTime? UsedDate { get; set; }

    public string Status { get; set; } = null!;

    public Guid VoucherId { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Voucher Voucher { get; set; } = null!;
}
