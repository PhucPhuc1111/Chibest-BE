using Chibest.Common;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chibest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _roleService.GetPagedAsync(pageNumber, pageSize, search);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _roleService.GetAllRolesAsync();
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await _roleService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpGet("{id}/with-accounts")]
    public async Task<IActionResult> GetWithAccounts([FromRoute] Guid id)
    {
        var result = await _roleService.GetRoleWithAccountsAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoleRequest request)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _roleService.CreateAsync(request, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpPost("account-role")]
    public async Task<IActionResult> CreateAccountRole([FromBody] AccountRoleRequest request)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _roleService.CreateAccountRoleAsync(request, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] RoleRequest request)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _roleService.UpdateAsync(request, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpPatch("account-role")]
    public async Task<IActionResult> ChangeAccountRole([FromBody] AccountRoleRequest request)
    {
        var whoMakeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (whoMakeId == null || whoMakeId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _roleService.ChangeRoleAccountAsync(request, Guid.Parse(whoMakeId));
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _roleService.DeleteAsync(id, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpDelete("account-role")]
    public async Task<IActionResult> DeleteAccountRole([FromQuery] Guid accountId, [FromQuery] Guid roleId)
    {
        var makerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (makerId == null || makerId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _roleService.DeleteAccountRoleAsync(accountId, roleId, Guid.Parse(makerId));
        return StatusCode(result.StatusCode, result);
    }
}