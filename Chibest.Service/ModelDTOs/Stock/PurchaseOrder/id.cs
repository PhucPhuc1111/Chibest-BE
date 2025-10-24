using Chibest.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Stock.PurchaseOrder
{
    public class PurchaseOrderResponse
    {
        public Guid Id { get; set; }

        public string InvoiceCode { get; set; } = null!;

        public DateTime OrderDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public decimal SubTotal { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal Paid { get; set; }

        public string? Note { get; set; }

        public string Status { get; set; } = null!;

        public string? WarehouseName { get; set; }

        public string? EmployeeName { get; set; }

        public string? SupplierName { get; set; }

        public virtual ICollection<PurchaseOrderDetailResponse> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetailResponse>();
    }

    public class PurchaseOrderDetailResponse
    {
        public Guid Id { get; set; }

        public int Quantity { get; set; }

        public int? ActualQuantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }

        public decimal ReFee { get; set; }

        public string? Note { get; set; }

        public string? ProductName { get; set; }

        public string Sku { get; set; } = null!;

    }
}