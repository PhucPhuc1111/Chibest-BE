using Chibest.Common.BusinessResult;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Chibest.Service.ModelDTOs.Stock.TransferOrder.create;
using static Chibest.Service.ModelDTOs.Stock.TransferOrder.update;

namespace Chibest.Service.Interface
{
    public interface ITransferOrderService
    {
        Task<IBusinessResult> AddTransferOrder(TransferOrderCreate request);
        Task<IBusinessResult> GetTransferOrderList(int pageIndex, int pageSize, string? search = null,
    DateTime? fromDate = null, DateTime? toDate = null, string? status = null, Guid? fromWarehouseId = null,
    Guid? toWarehouseId = null, Guid? branchId = null);
        Task<IBusinessResult> GetTransferOrderById(Guid id);
        Task<IBusinessResult> UpdateTransferOrderAsync(Guid id, TransferOrderUpdate request);
        Task<IBusinessResult> ReadTransferDetailFromExcel(IFormFile file);
        Task<IBusinessResult> AddMultiTransferOrder(TransferMultiOrderCreate request);
        Task<IBusinessResult> DeleteTransferOrder(Guid id);
    }
}
