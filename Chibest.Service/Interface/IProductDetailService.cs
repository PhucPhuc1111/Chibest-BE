using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;

namespace Chibest.Service.Interface;

public interface IProductDetailService
{
    Task<IBusinessResult> GetListAsync(ProductDetailQuery query);
    Task<IBusinessResult> GetByIdAsync(Guid id);
    Task<IBusinessResult> GetByCodeAsync(string? chipCode, string? barcode, string? tagId);
    Task<IBusinessResult> CreateAsync(ProductDetailRequest request, Guid accountId);
    Task<IBusinessResult> UpdateAsync(ProductDetailRequest request, Guid accountId);
    Task<IBusinessResult> UpdateStatusAsync(Guid id, Guid accountId, string status);
    Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId);
}