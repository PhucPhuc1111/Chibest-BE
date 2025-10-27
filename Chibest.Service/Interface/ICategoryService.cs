using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;

namespace Chibest.Service.Interface;

public interface ICategoryService
{
    Task<IBusinessResult> GetListAsync(CategoryQuery query);
    Task<IBusinessResult> GetByIdAsync(Guid id);
    Task<IBusinessResult> GetByTypeAsync(string type);
    Task<IBusinessResult> GetHierarchyAsync();
    Task<IBusinessResult> GetChildrenAsync(Guid parentId);
    Task<IBusinessResult> CreateAsync(CategoryRequest request, Guid accountId);
    Task<IBusinessResult> UpdateAsync(CategoryRequest request, Guid accountId);
    Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId);
}