using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;

namespace Chibest.Service.Interface;

public interface IProductPlanService
{
    Task<IBusinessResult> GetListAsync(ProductPlanQuery query);
    Task<IBusinessResult> GetByIdAsync(Guid id);
    Task<IBusinessResult> CreateAsync(ProductPlanRequest request, Guid accountId);
    Task<IBusinessResult> UpdateAsync(ProductPlanRequest request, Guid accountId);
    Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId);
}

