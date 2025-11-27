using System;
using Chibest.API.Attributes;
using Chibest.Common;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Mvc;

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
    [Permission(Const.Permissions.File)]
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

    [HttpGet("video")]
    public IActionResult GetVideo([FromQuery] string urlPath)
    {
        if (string.IsNullOrEmpty(urlPath))
            return BadRequest("Đường dẫn file là bắt buộc.");

        try
        {
            var (fileStream, contentType, _) = _fileService.GetVideoFile(urlPath);
            Response.Headers["Accept-Ranges"] = "bytes";

            return File(fileStream, contentType, fileDownloadName: null, enableRangeProcessing: true);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [Permission(Const.Permissions.File)]
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
