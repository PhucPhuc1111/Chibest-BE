using Chibest.Common.BusinessResult;
using Microsoft.AspNetCore.Http;

namespace Chibest.Service.Interface;

public interface IFileService
{
    Task<string> SaveImageAsync(IFormFile imageFile, string fileName, string category);
    (Stream FileStream, string ContentType) GetImageFileAsync(string relativePath);
    Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string mappingKey) where T : class;
    Task<List<T>> ImportFromExcelAsync<T>(IFormFile file, string mappingKey) where T : class, new();
}