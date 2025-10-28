using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Stock.TransferOrder
{
    public class list
    {
        public class TransferOrderList
        {
            public Guid Id { get; set; }

            public string InvoiceCode { get; set; } = null!;

            public string? FromWarehouseName { get; set; }

            public string? ToWarehouseName { get; set; }

            public DateTime OrderDate { get; set; }

            public decimal SubTotal { get; set; }

            public string Status { get; set; } = null!;
        }
    }
}
