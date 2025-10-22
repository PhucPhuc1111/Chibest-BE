using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;

namespace Chibest.Service.Interface;

public interface IBranchStockService
{
    Task<IBusinessResult> GetByIdAsync(Guid id);
    Task<IBusinessResult> GetPagedAsync(int pageNumber, int pageSize, Guid? productId = null, Guid? branchId = null);
    Task<IBusinessResult> CreateAsync(BranchStockRequest request);
    Task<IBusinessResult> UpdateAsync(Guid id, BranchStockRequest request);
    Task<IBusinessResult> DeleteAsync(Guid id);
    Task<IBusinessResult> UpdateStockAsync(Guid id, int availableQty, int reservedQty);
    Task<IBusinessResult> GetByProductAndBranchAsync(Guid productId, Guid branchId);
    Task<IBusinessResult> GetLowStockItemsAsync(Guid branchId);
}
