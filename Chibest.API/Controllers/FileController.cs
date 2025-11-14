using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sprache;
using System.Security.Claims;

namespace Chibest.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;

    public FileController(IFileService fileService)
    {
        _fileService = fileService;
    }

    //=================================[ Endpoints ]================================
    [Authorize]
    [HttpPost("image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage([FromForm] ImageRequest request)
    {
        var relativePath = await _fileService.SaveImageAsync(request.FileData, request.Name, request.Category);

        return Ok(relativePath);
    }

    [HttpGet("image")]
    public IActionResult GetImage([FromQuery] string urlPath)
    {
        if (string.IsNullOrEmpty(urlPath))
            return BadRequest("Đường dẫn file là bắt buộc.");

        var (fileStream, contentType) = _fileService.GetImageFile(urlPath);

        // Trả về file stream, trình duyệt sẽ tự hiển thị
        return File(fileStream, contentType);
    }

    [Authorize]
    [HttpPost("export")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> ExportProducts([FromBody] ExcelExportRequest request)
    {
        var fileBytes = await _fileService.ExportProductsToExcelAsync(request);

        // Đặt tên file
        var fileName = $"Products_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

        // Đặt ContentType cho file Excel (XLSX)
        var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        // Trả về file
        return File(fileBytes, contentType, fileName);
    }
}
