using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;

namespace Chibest.Service.Interface;

public interface IProductPriceHistoryService
{
    Task<IBusinessResult> GetListAsync(ProductPriceHistoryQuery query, string? productName = null);
    Task<IBusinessResult> GetByIdAsync(Guid id);
    Task<IBusinessResult> GetCurrentPricesAsync(Guid? branchId = null);
    Task<IBusinessResult> GetByProductIdAsync(Guid productId);
    Task<IBusinessResult> GetByBranchIdAsync(Guid branchId);
    Task<IBusinessResult> CreateAsync(ProductPriceHistoryRequest request, Guid accountId);
    Task<IBusinessResult> UpdateAsync(ProductPriceHistoryRequest request, Guid accountId);
    Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId);
}