using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class TransferOrder
{
    public Guid Id { get; set; }

    public string InvoiceCode { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ActualDeliveryDate { get; set; }

    public string? PayMethod { get; set; }

    public decimal SubTotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal Paid { get; set; }

    public string? Note { get; set; }

    public string Status { get; set; } = null!;

    public Guid? EmployeeId { get; set; }

    public Guid? FromWarehouseId { get; set; }

    public Guid? ToWarehouseId { get; set; }

    public virtual Account? Employee { get; set; }

    public virtual Warehouse? FromWarehouse { get; set; }

    public virtual Warehouse? ToWarehouse { get; set; }

    public virtual ICollection<TransferOrderDetail> TransferOrderDetails { get; set; } = new List<TransferOrderDetail>();
}
