using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Stock.TransferOrder
{
    public class id
    {
        public class TransferOrderResponse
        {
            public Guid Id { get; set; }

            public string InvoiceCode { get; set; } = null!;

            public DateTime OrderDate { get; set; }

            public DateTime CreatedAt { get; set; }

            public DateTime UpdatedAt { get; set; }

            public decimal SubTotal { get; set; }

            public string? Note { get; set; }

            public string Status { get; set; } = null!;

            public string? FromWarehouseName { get; set; }

            public string? ToWarehouseName { get; set; }

            public virtual ICollection<TransferOrderDetailResponse> TransferOrderDetails { get; set; } = new List<TransferOrderDetailResponse>();
        }

        public class TransferOrderDetailResponse
        {
            public Guid Id { get; set; }

            public string ContainerCode { get; set; } = null!;

            public int Quantity { get; set; }

            public int? ActualQuantity { get; set; }

            public decimal CommissionFee { get; set; }

            public decimal ExtraFee { get; set; }

            public decimal UnitPrice { get; set; }


            public string? Note { get; set; }

            public string? ProductName { get; set; }

            public string Sku { get; set; } = null!;

        }
    }
}
