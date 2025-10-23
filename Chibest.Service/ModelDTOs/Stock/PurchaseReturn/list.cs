using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Stock.PurchaseReturn
{
    public class list
    {
        public class PurchaseReturnList
        {
            public Guid Id { get; set; }

            public string InvoiceCode { get; set; } = null!;

            public DateTime OrderDate { get; set; }

            public decimal SubTotal { get; set; }

            public string Status { get; set; } = null!;
        }
    }
}
