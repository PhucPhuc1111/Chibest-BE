using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Stock.TransferOrder
{
    public class update
    {
        public class TransferOrderUpdate
        {
            public string Status { get; set; } = null!;
            public virtual ICollection<TransferOrderDetailUpdate> TransferOrderDetails { get; set; } = new List<TransferOrderDetailUpdate>();
        }
        public class TransferOrderDetailUpdate
        {
            public Guid Id { get; set; }
            public int? ActualQuantity { get; set; }
        }
    }
}
