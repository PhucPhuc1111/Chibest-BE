using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Http;

namespace Chibest.Service.Interface;

public interface IFileService
{
    Task<string> SaveImageAsync(IFormFile imageFile, string fileName, string category);
    (Stream FileStream, string ContentType) GetImageFileAsync(string relativePath);
    Task<byte[]> ExportProductsToExcelAsync(ExcelExportRequest request);
}