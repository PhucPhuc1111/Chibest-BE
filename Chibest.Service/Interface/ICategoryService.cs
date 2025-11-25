using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;

namespace Chibest.Service.Interface;

public interface ICategoryService
{
    Task<IBusinessResult> GetListAsync(CategoryQuery query);
    Task<IBusinessResult> GetByIdAsync(Guid id);
    Task<IBusinessResult> CreateAsync(CategoryRequest request);
    Task<IBusinessResult> UpdateAsync(CategoryRequest request);
    Task<IBusinessResult> DeleteAsync(Guid id);
}