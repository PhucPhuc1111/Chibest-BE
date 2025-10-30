using Chibest.Common;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chibest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    //=================================[ Endpoints ]================================
    //--------------------------------------------------------------------------------------------------------
    //Authentication
    //--------------------------------------------------------------------------------------------------------
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest input)
    {
        var result = await _accountService.LoginByPasswordAsync(input);
        return StatusCode(result.StatusCode, result);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] AuthTokenRequest request)
    {
        var result = await _accountService.RefreshTokenAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        await _accountService.RevokeRefreshTokenAsync(Guid.Parse(accountId));
        return Ok(new { message = "Logged out successfully" });
    }
    //--------------------------------------------------------------------------------------------------------

    //[Authorize(Roles = "admin,moderator")]
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> ViewAccount([FromRoute] Guid id)
    {
        var result = await _accountService.GetAccountByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("list")]
    public async Task<IActionResult> GetListAccount(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _accountService.GetAccountsListAsync(pageNumber, pageSize, search);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateAccount([FromBody] AccountRequest request)
    {
        var result = await _accountService.UpdateAccountAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    //[Authorize]
    //[HttpPatch("password-recovery")]
    //public async Task<IActionResult> ResetPassword()
    //{
    //    ...Implement password recovery logic here
    //    return Ok();
    //}

    [Authorize]
    [HttpPatch("new-password")]
    public async Task<IActionResult> ChangePassword([FromBody] string newPassword)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId.Equals(Guid.Empty.ToString()))
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _accountService.ChangeAccountPasswordAsync(Guid.Parse(accountId), newPassword);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("temporary")]
    public async Task<IActionResult> Delete()
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _accountService.ChangeAccountStatusAsync(Guid.Parse(accountId),"Chờ Xóa");
        return StatusCode(result.StatusCode, result);
    }

    //----------------------------------------------------------------------------
    //==============================[ ADMIN APIs ]================================
    [Authorize(Roles = Const.Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] AccountRequest newAccount)
    {
        var result = await _accountService.CreateAccountAsync(newAccount);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = Const.Roles.Admin)]
    [HttpDelete]
    public async Task<IActionResult> DeleteByAdmin()
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _accountService.DeleteAccountAsync(Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("supplier")]
    public async Task<IActionResult> ViewSupplier()
    {
        var result = await _accountService.GetSupplierAccountsAsync();
        return StatusCode(result.StatusCode, result);
    }
}