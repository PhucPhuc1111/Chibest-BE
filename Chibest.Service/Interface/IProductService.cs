using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;

namespace Chibest.Service.Interface;

public interface IProductService
{
    Task<IBusinessResult> GetListAsync(ProductQuery query);
    Task<IBusinessResult> GetByIdAsync(Guid id);
    Task<IBusinessResult> GetBySKUAsync(string sku);
    Task<IBusinessResult> CreateAsync(ProductRequest request, Guid accountId);
    Task<IBusinessResult> UpdateAsync(ProductRequest request, Guid accountId);
    Task<IBusinessResult> UpdateStatusAsync(Guid id, Guid accountId, string status);
    Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId);
}
