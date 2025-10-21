using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;

namespace Chibest.Service.Interface;

public interface ICategoryService
{
    Task<IBusinessResult> GetByIdAsync(Guid id);
    Task<IBusinessResult> GetPagedAsync(int pageNumber, int pageSize, string? search = null, string? type = null);
    Task<IBusinessResult> CreateAsync(CategoryRequest request);
    Task<IBusinessResult> UpdateAsync(Guid id, CategoryRequest request);
    Task<IBusinessResult> DeleteAsync(Guid id);
    Task<IBusinessResult> GetCategoriesWithProductsAsync();
}