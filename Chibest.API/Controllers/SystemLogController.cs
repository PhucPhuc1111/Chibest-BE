using Chibest.Common;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chibest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemLogsController : ControllerBase
{
    private readonly ISystemLogService _systemLogService;

    public SystemLogsController(ISystemLogService systemLogService)
    {
        _systemLogService = systemLogService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? logLevel = null, [FromQuery] string? module = null)
    {
        var result = await _systemLogService.GetPagedAsync(pageNumber, pageSize, search, logLevel, module);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _systemLogService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SystemLogRequest request)
    {
        var result = await _systemLogService.CreateAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _systemLogService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpDelete("cleanup")]
    public async Task<IActionResult> DeleteOldLogs([FromQuery] DateTime olderThan)
    {
        var result = await _systemLogService.DeleteOldLogsAsync(olderThan);
        return StatusCode(result.StatusCode, result);
    }
}
