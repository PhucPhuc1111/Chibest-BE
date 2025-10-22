using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.PurchaseOrder;
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
        Task<IBusinessResult> GetPurchaseOrderList(int pageIndex, int pageSize, string search);
        Task<IBusinessResult> UpdateAsync(Guid id, PurchaseOrderUpdate request);
        Task<IBusinessResult> GetPurchaseOrderById(Guid id);
    }
}
