using Chibest.API.Attributes;
using Chibest.Common;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chibest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Permission(Const.Permissions.Product)]
public class ProductDetailController : ControllerBase
{
    private readonly IProductDetailService _productDetailService;

    public ProductDetailController(IProductDetailService productDetailService)
    {
        _productDetailService = productDetailService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] ProductDetailQuery query)
    {
        var result = await _productDetailService.GetListAsync(query);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await _productDetailService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("code")]
    public async Task<IActionResult> GetByProductAndBranch(
        [FromQuery] string? chipCode,
        [FromQuery] string? barCode,
        [FromQuery] string? tagId)
    {
        var result = await _productDetailService.GetByCodeAsync(chipCode, barCode, tagId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductDetailRequest request)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _productDetailService.CreateAsync(request, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ProductDetailRequest request)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _productDetailService.UpdateAsync(request, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] string status)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _productDetailService.UpdateStatusAsync(id, Guid.Parse(accountId), status);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _productDetailService.DeleteAsync(id, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }
}
