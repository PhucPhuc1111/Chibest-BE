using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class CustomerVoucher
{
    public DateTime CollectedDate { get; set; }

    public DateTime? UsedDate { get; set; }

    public string Status { get; set; } = null!;

    public Guid VoucherId { get; set; }

    public Guid AccountId { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Voucher Voucher { get; set; } = null!;
}
