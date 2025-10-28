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
            public string? PayMethod { get; set; }
            public decimal SubTotal { get; set; }

            public decimal DiscountAmount { get; set; } = 0;
            public decimal Paid { get; set; } = 0;
            public OrderStatus Status { get; set; } 
            public virtual ICollection<TransferOrderDetailUpdate> TransferOrderDetails { get; set; } = new List<TransferOrderDetailUpdate>();
        }
        public class TransferOrderDetailUpdate
        {
            public Guid Id { get; set; }
            public decimal ExtraFee { get; set; } = 0;
            public decimal CommissionFee { get; set; } = 0;
            public int? ActualQuantity { get; set; }
        }
    }
}
