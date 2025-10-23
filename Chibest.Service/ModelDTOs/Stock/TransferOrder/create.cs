using Chibest.Service.ModelDTOs.Stock.PurchaseOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Stock.TransferOrder
{
    public class create
    {
        public class TransferOrderCreate
        {
            public string? InvoiceCode { get; set; }
            public DateTime OrderDate { get; set; }

            public string? PayMethod { get; set; }

            public decimal SubTotal { get; set; }

            public decimal DiscountAmount { get; set; }

            public decimal Paid { get; set; }

            public string? Note { get; set; }

            public Guid? FromWarehouseId { get; set; }

            public Guid? ToWarehouseId { get; set; }

            public Guid? EmployeeId { get; set; }
            public virtual ICollection<TransferOrderCreateDetailCreate> TransferOrderDetails { get; set; } = new List<TransferOrderCreateDetailCreate>();
        }
        public class TransferOrderCreateDetailCreate
        {

            public int Quantity { get; set; }

            public decimal UnitPrice { get; set; }

            public decimal Discount { get; set; }

            public string? Note { get; set; }

            public Guid ProductId { get; set; }
        }
    }
}

