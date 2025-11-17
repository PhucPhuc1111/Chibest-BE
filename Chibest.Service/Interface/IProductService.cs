using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;
using Microsoft.AspNetCore.Http;

namespace Chibest.Service.Interface;

public interface IProductService
{
    
    Task<IBusinessResult> GetMasterListAsync(ProductQuery query);
    Task<IBusinessResult> GetVariantsByParentSkuAsync(string parentSku, Guid? branchId = null);
    Task<IBusinessResult> GetListAsync(ProductQuery query);
    Task<IBusinessResult> CreateAsync(ProductRequest request, Guid accountId);
    //Task<IBusinessResult> UpdateAsync(ProductRequest request, Guid accountId);
    Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId);
    Task<IBusinessResult> GetByIdAsync(Guid id, Guid? branchId);
    //Task<IBusinessResult> ImportProductsFromExcelAsync(IFormFile file, Guid accountId);
}
