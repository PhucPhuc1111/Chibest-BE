using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;

namespace Chibest.Service.Interface;

public interface IProductService
{
    Task<IBusinessResult> GetByIdAsync(Guid id);
    Task<IBusinessResult> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        Guid? categoryId = null,
        string? status = null);
    Task<IBusinessResult> CreateAsync(ProductRequest request);
    Task<IBusinessResult> UpdateAsync(Guid id, ProductRequest request);
    Task<IBusinessResult> DeleteAsync(Guid id);
    Task<IBusinessResult> UpdateStatusAsync(Guid id, string status);
}
