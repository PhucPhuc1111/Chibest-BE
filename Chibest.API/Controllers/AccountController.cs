using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chibest.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    //=================================[ Endpoints ]================================
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> ViewAccount([FromRoute] Guid id)
    {
        var result = await _accountService.GetAccountByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    //----------------------------------------------------------------------------
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest input)
    {
        var result = await _accountService.LoginByPasswordAsync(input);
        return StatusCode(result.StatusCode, result);
    }
}