using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class Customer
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? AvatarUrl { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime LastActive { get; set; }

    public Guid? GroupId { get; set; }

    public virtual CustomerDebt? CustomerDebt { get; set; }

    public virtual ICollection<CustomerDebtHistory> CustomerDebtHistories { get; set; } = new List<CustomerDebtHistory>();

    public virtual ICollection<CustomerVoucher> CustomerVouchers { get; set; } = new List<CustomerVoucher>();

    public virtual Customer? Group { get; set; }

    public virtual ICollection<Customer> InverseGroup { get; set; } = new List<Customer>();

    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    public virtual ICollection<SalesReturn> SalesReturns { get; set; } = new List<SalesReturn>();
}
