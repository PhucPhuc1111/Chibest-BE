using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class Voucher
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string VoucherType { get; set; } = null!;

    public DateTime AvailableDate { get; set; }

    public DateTime ExpiredDate { get; set; }

    public decimal MinimumTransaction { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public decimal? DiscountPercent { get; set; }

    public decimal? DiscountAmount { get; set; }

    public int? UsageLimit { get; set; }

    public int? UsagePerCustomer { get; set; }

    public int UsedCount { get; set; }

    public string? ApplicableProducts { get; set; }

    public string? ApplicableCategories { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CustomerVoucher> CustomerVouchers { get; set; } = new List<CustomerVoucher>();

    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
}
