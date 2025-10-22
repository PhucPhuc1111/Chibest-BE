using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;

namespace Chibest.Service.Interface;

public interface IProductDetailService
{
    Task<IBusinessResult> GetByIdAsync(Guid id);
    Task<IBusinessResult> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? productId = null,
        Guid? branchId = null,
        string? status = null);
    Task<IBusinessResult> CreateAsync(ProductDetailRequest request);
    Task<IBusinessResult> UpdateAsync(Guid id, ProductDetailRequest request);
    Task<IBusinessResult> DeleteAsync(Guid id);
    Task<IBusinessResult> GetByProductAndBranchAsync(Guid productId, Guid branchId);
}