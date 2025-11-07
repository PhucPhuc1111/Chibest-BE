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
            public virtual ICollection<TransferOrderDetailCreate> TransferOrderDetails { get; set; } = new List<TransferOrderDetailCreate>();
        }
        public class TransferOrderDetailCreate
        {

            public int Quantity { get; set; }

            public decimal UnitPrice { get; set; }

            public decimal ExtraFee { get; set; }

            public decimal CommissionFee { get; set; }

            public decimal Discount { get; set; }

            public string? Note { get; set; }

            public Guid ProductId { get; set; }
        }

        public class TransferMultiOrderCreate
        {
            public Guid FromWarehouseId { get; set; }
            public Guid EmployeeId { get; set; }
            public DateTime OrderDate { get; set; }
            public string? Note { get; set; }
            public string? PayMethod { get; set; }
            public decimal DiscountAmount { get; set; }
            public decimal Paid { get; set; }

            // Danh sách chuyển hàng đến nhiều chi nhánh
            public List<BranchTransferInfo> Destinations { get; set; } = new();
        }

        /// <summary>
        /// Thông tin hàng hóa chuyển đến từng chi nhánh
        /// </summary>
        public class BranchTransferInfo
        {
            public Guid ToWarehouseId { get; set; }
            public decimal SubTotal { get; set; }

            // Danh sách sản phẩm + phí riêng của chi nhánh đó
            public List<BranchProductTransfer> Products { get; set; } = new();
        }

        /// <summary>
        /// Mỗi sản phẩm có thể có phí riêng cho từng chi nhánh
        /// </summary>
        public class BranchProductTransfer
        {
            public Guid ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal ExtraFee { get; set; }
            public decimal CommissionFee { get; set; }
            public decimal Discount { get; set; }
            public string? Note { get; set; }
        }

    }
}

