using Chibest.Common.BusinessResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Chibest.Service.ModelDTOs.Stock.StockAdjustment.create;
using static Chibest.Service.ModelDTOs.Stock.StockAdjustment.update;

namespace Chibest.Service.Interface
{
    public interface IStockAdjusmentService
    {
        Task<IBusinessResult> GetStockAdjustmentById(Guid id);
        Task<IBusinessResult> GetStockAdjustmentList(
    int pageIndex,
    int pageSize,
    string? search,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    string? status = null,
    string? adjustmentType = null,
    Guid? branchId = null);
        Task<IBusinessResult> AddStockAdjustment(StockAdjustmentCreate request);
        Task<IBusinessResult> UpdateStockAdjustment(Guid id, StockAdjustmentUpdate request);
        Task<IBusinessResult> DeleteStockAdjustment(Guid id);
    }
}
