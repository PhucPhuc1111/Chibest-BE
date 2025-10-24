using Chibest.Common;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chibest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] ProductQuery query)
    {
        var result = await _productService.GetListAsync(query);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _productService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("sku/{sku}")]
    public async Task<IActionResult> GetBySKU(string sku)
    {
        var result = await _productService.GetBySKUAsync(sku);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductRequest request)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _productService.CreateAsync(request, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductRequest request)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _productService.UpdateAsync(request, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] string status)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _productService.UpdateStatusAsync(id, Guid.Parse(accountId), status);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _productService.DeleteAsync(id, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }
}