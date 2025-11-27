using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Chibest.Service.Interface;

public interface IProductService
{
    
    Task<IBusinessResult> GetMasterListAsync(ProductQuery query);
    Task<IBusinessResult> GetVariantsByParentSkuAsync(string parentSku, Guid? branchId = null);
    Task<IBusinessResult> GetListAsync(ProductQuery query);
    Task<IBusinessResult> CreateAsync(ProductRequest request, Guid accountId);
    Task<IBusinessResult> UpdateProductFieldsAsync(
        Guid productId,
        IFormFile? avatarFile = null,
        IFormFile? videoFile = null,
        decimal? costPrice = null,
        decimal? sellingPrice = null,
        string? name = null,
        string? status = null,
        string? description = null);
    Task<IBusinessResult> DeleteAsync(IEnumerable<Guid> productIds);
    Task<IBusinessResult> GetByIdAsync(Guid id, Guid? branchId);
    //Task<IBusinessResult> ImportProductsFromExcelAsync(IFormFile file, Guid accountId);
    Task<IBusinessResult> GenerateProductBarcodeAsync(Guid productId, Guid? branchId);
}
