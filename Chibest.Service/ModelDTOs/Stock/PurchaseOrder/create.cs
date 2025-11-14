using Chibest.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Stock.PurchaseOrder
{
    public class PurchaseOrderCreate
    {
        public string? InvoiceCode { get; set; }
        public DateTime OrderDate { get; set; }

        public decimal SubTotal { get; set; }

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

        public decimal ReFee { get; set; }

        public string? Note { get; set; }

        public Guid ProductId { get; set; }
    }
}
