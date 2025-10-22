using Chibest.Common;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        var result = await _roleService.CreateAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] RoleRequest request)
    {
        var result = await _roleService.UpdateAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpPatch("{roleId}/{accountId}")]
    public async Task<IActionResult> ChangeAccountRole([FromRoute] Guid accountId, [FromRoute] Guid roleId)
    {
        var result = await _roleService.ChangeRoleAccountAsync(accountId, roleId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await _roleService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}