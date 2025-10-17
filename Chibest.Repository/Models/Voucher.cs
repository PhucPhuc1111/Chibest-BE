using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class Voucher
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime AvailableDate { get; set; }

    public DateTime ExpiredDate { get; set; }

    public decimal MinimumTransaction { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public byte DiscountPercent { get; set; }

    public int? UsageLimit { get; set; }

    public int UsedCount { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<CustomerVoucher> CustomerVouchers { get; set; } = new List<CustomerVoucher>();

    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
}
