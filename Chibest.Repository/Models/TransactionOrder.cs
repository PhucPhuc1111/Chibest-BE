using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class TransactionOrder
{
    public Guid Id { get; set; }

    public string InvoiceCode { get; set; } = null!;

    public string? Type { get; set; }

    public DateTime OrderDate { get; set; }

    public string ReceiverName { get; set; } = null!;

    public string? ReceiverPhone { get; set; }

    public string ReceiverAddress { get; set; } = null!;

    public DateTime? ExpectedDeliveryDate { get; set; }

    public DateTime? ActualDeliveryDate { get; set; }

    public string PayMethod { get; set; } = null!;

    public decimal SubTotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal? FinalCost { get; set; }

    public string? Note { get; set; }

    public string Status { get; set; } = null!;

    public Guid? FromWarehouseId { get; set; }

    public Guid? ToWarehouseId { get; set; }

    public Guid? EmployeeId { get; set; }

    public Guid? SupplierId { get; set; }

    public virtual Account? Employee { get; set; }

    public virtual Warehouse? FromWarehouse { get; set; }

    public virtual Account? Supplier { get; set; }

    public virtual Warehouse? ToWarehouse { get; set; }

    public virtual ICollection<TransactionOrderDetail> TransactionOrderDetails { get; set; } = new List<TransactionOrderDetail>();
}
