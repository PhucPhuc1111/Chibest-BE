using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class Fee
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Cost { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? PurchaseOrderId { get; set; }

    public Guid? TransferOrderId { get; set; }

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual PurchaseOrder? PurchaseOrder { get; set; }

    public virtual TransferOrder? TransferOrder { get; set; }
}
