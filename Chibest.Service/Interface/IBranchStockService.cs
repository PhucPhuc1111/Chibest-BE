using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;

namespace Chibest.Service.Interface;

public interface IBranchStockService
{
    Task<IBusinessResult> GetListAsync(BranchStockQuery query);
    Task<IBusinessResult> GetByIdAsync(Guid id);
    Task<IBusinessResult> GetByProductAndBranchAsync(Guid productId, Guid branchId);
    Task<IBusinessResult> GetLowStockItemsAsync(Guid? branchId = null);
    Task<IBusinessResult> GetItemsNeedingReorderAsync(Guid? branchId = null);
    Task<IBusinessResult> CreateAsync(BranchStockRequest request, Guid accountId);
    Task<IBusinessResult> UpdateAsync(BranchStockRequest request, Guid accountId);
    Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId);
}

