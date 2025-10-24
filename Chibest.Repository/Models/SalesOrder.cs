using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class SalesOrder
{
    public Guid Id { get; set; }

    public string OrderCode { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public Guid CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string? CustomerPhone { get; set; }

    public string? CustomerEmail { get; set; }

    public Guid BranchId { get; set; }

    public Guid? WarehouseId { get; set; }

    public Guid EmployeeId { get; set; }

    public string DeliveryMethod { get; set; } = null!;

    public string? ShippingAddress { get; set; }

    public string? ShippingPhone { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }

    public DateTime? ActualDeliveryDate { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public decimal SubTotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public Guid? VoucherId { get; set; }

    public decimal VoucherAmount { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal? FinalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public string Status { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Account Employee { get; set; } = null!;

    public virtual ICollection<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();

    public virtual Voucher? Voucher { get; set; }

    public virtual Warehouse? Warehouse { get; set; }
}
