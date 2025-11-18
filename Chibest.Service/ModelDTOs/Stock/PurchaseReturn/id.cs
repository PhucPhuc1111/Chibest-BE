using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Stock.PurchaseReturn
{
    public class id
    {
        public class PurchaseReturnResponse
        {
            public Guid Id { get; set; }

            public string InvoiceCode { get; set; } = null!;

            public DateTime OrderDate { get; set; }

            public DateTime CreatedAt { get; set; }

            public DateTime UpdatedAt { get; set; }

            public decimal SubTotal { get; set; }


            public string? Note { get; set; }

            public string Status { get; set; } = null!;

            public string? BranchName { get; set; }

            public string? SupplierName { get; set; }

            public virtual ICollection<PurchaseReturnDetailResponse> PurchaseReturnDetails { get; set; } = new List<PurchaseReturnDetailResponse>();
        }

        public class PurchaseReturnDetailResponse
        {
            public Guid Id { get; set; }

            public string ContainerCode { get; set; } = null!;

            public int Quantity { get; set; }

            public decimal UnitPrice { get; set; }

            public string? Note { get; set; }

            public string? ProductName { get; set; }

            public string Sku { get; set; } = null!;

        }
    }
}

