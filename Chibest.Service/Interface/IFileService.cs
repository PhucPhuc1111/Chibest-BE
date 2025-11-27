using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Http;

namespace Chibest.Service.Interface;

public interface IFileService
{
    Task<string> SaveImageAsync(IFormFile imageFile, string fileName, string category);
    Task<string> SaveProductImageAsync(IFormFile imageFile, string sku);
    Task<string> SaveProductVideoAsync(IFormFile videoFile, string sku);
    (Stream FileStream, string ContentType) GetImageFile(string relativePath);
    (Stream FileStream, string ContentType, long FileLength) GetVideoFile(string relativePath);
    Task<byte[]> ExportProductsToExcelAsync(ExcelExportRequest request);
}