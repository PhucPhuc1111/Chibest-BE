using Chibest.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Stock.TransferOrder
{
    public class update
    {
        public class TransferOrderUpdate
        {
            public decimal SubTotal { get; set; }

            public OrderStatus Status { get; set; } 
            public virtual ICollection<TransferOrderDetailUpdate> TransferOrderDetails { get; set; } = new List<TransferOrderDetailUpdate>();
        }
        public class TransferOrderDetailUpdate
        {
            public Guid Id { get; set; }
            public decimal CommissionFee { get; set; } = 0;
            public decimal UnitPrice { get; set; }
            public int? ActualQuantity { get; set; }
            public string? Note { get; set; }

        }
    }
}
