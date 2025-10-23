using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Stock.PurchaseOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.Interface
{
    public interface IPurchaseOrderService
    {
        Task<IBusinessResult> AddPurchaseOrder(PurchaseOrderCreate request);
        Task<IBusinessResult> GetPurchaseOrderList(int pageIndex, int pageSize, string search,
    DateTime? fromDate = null, DateTime? toDate = null, string status = null);
        Task<IBusinessResult> UpdateAsync(Guid id, PurchaseOrderUpdate request);
        Task<IBusinessResult> GetPurchaseOrderById(Guid id);
    }
}
