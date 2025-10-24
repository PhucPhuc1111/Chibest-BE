using Chibest.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Stock.PurchaseOrder
{
    public class PurchaseOrderUpdate
    {
        public OrderStatus Status { get; set; }
        public virtual ICollection<PurchaseOrderDetailUpdate> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetailUpdate>();
    }
    public class PurchaseOrderDetailUpdate
    {
        public Guid Id { get; set; }
        public int? ActualQuantity { get; set; }
    }
}
