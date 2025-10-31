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
    public async Task<IActionResult> UploadAvatar([FromForm] ImageRequest request)
    {
        try
        {
            var response = await _fileService.SaveImageAsync(request.FileData,request.Name,request.Category);

            return StatusCode(response.StatusCode, response);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, "Lỗi hệ thống."); }
    }

    [Authorize]
    [HttpGet("image")]
    public async Task<IActionResult> GetImage([FromQuery] string urlPath)
    {
        if (string.IsNullOrEmpty(urlPath))
            return BadRequest("Đường dẫn file là bắt buộc.");

        try
        {

            var (fileStream, contentType) = _fileService.GetImageFileAsync(urlPath);

            // Trả về file stream, trình duyệt sẽ tự hiển thị
            return File(fileStream, contentType);
        }
        catch (FileNotFoundException) { return NotFound("Không tìm thấy file."); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (Exception) { return StatusCode(500, "Lỗi hệ thống."); }
    }

    [HttpGet("export/accounts")]
    public async Task<IActionResult> ExportAccounts()
    {
        // Lấy data từ DB (ví dụ)
        var mockAccounts = new List<Account> { /* ... */ };

        // Chỉ cần truyền key "Account"
        var fileBytes = await _fileService.ExportToExcelAsync(mockAccounts, "Account");

        return File(fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "accounts.xlsx");
    }

    [HttpPost("import/accounts")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportAccounts(IFormFile file)
    {
            // Chỉ cần truyền key "Account" và Model
            var importedData = await _fileService.ImportFromExcelAsync<Account>(file, "Account");

            return Ok(new
            {
                message = $"Nhập thành công các tài khoản.",
                data = importedData
            });
    }
}
