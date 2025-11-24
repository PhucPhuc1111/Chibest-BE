using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public class CustomerVoucher
{
    public Guid VoucherId { get; set; }

    public Guid CustomerId { get; set; }

    public DateTime CollectedDate { get; set; }

    public DateTime? UsedDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Voucher Voucher { get; set; } = null!;
}
