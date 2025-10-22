using Chibest.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.PurchaseOrder
{
    public class PurchaseOrderCreate
    {
        public string InvoiceCode { get; set; } = null!; 

        public DateTime OrderDate { get; set; }

        public string? PayMethod { get; set; }

        public decimal SubTotal { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal Paid { get; set; }

        public string? Note { get; set; }

        public Guid? WarehouseId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? SupplierId { get; set; }


        public virtual ICollection<PurchaseOrderDetailCreate> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetailCreate>();

    }
    public class PurchaseOrderDetailCreate
    {

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }

        public string? Note { get; set; }

        public Guid ProductId { get; set; }
    }
}
