﻿using Chibest.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Stock.PurchaseOrder
{
    public class PurchaseOrderUpdate
    {
        public string? PayMethod { get; set; }

        public decimal SubTotal { get; set; }

        public decimal DiscountAmount { get; set; } = 0;

        public decimal Paid { get; set; } = 0;

        public OrderStatus Status { get; set; }
        public virtual ICollection<PurchaseOrderDetailUpdate> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetailUpdate>();
    }
    public class PurchaseOrderDetailUpdate
    {
        public Guid Id { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }

        public decimal ReFee { get; set; }
        public string? Note { get; set; }
        public int? ActualQuantity { get; set; }
    }
}
