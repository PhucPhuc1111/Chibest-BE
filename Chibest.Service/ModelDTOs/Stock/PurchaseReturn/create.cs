using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Stock.PurchaseReturn
{
    public class create
    {
        public class PurchaseReturnCreate
        {
            public string? InvoiceCode { get; set; }
            public DateTime OrderDate { get; set; }

            public string? PayMethod { get; set; }

            public decimal SubTotal { get; set; }

            public decimal DiscountAmount { get; set; }

            public decimal Paid { get; set; }

            public string? Note { get; set; }

            public Guid? WarehouseId { get; set; }

            public Guid? EmployeeId { get; set; }

            public Guid? SupplierId { get; set; }
            public virtual ICollection<PurchaseReturnDetailCreate> PurchaseReturnDetails { get; set; } = new List<PurchaseReturnDetailCreate>();
        }
        public class PurchaseReturnDetailCreate
        {

            public int Quantity { get; set; }

            public decimal UnitPrice { get; set; }

            public decimal ReturnPrice { get; set; }

            public string? Note { get; set; }

            public Guid ProductId { get; set; }
        }
    }
}