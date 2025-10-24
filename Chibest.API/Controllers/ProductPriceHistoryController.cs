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
public class ProductPriceHistoryController : ControllerBase
{
    private readonly IProductPriceHistoryService _priceHistoryService;

    public ProductPriceHistoryController(IProductPriceHistoryService priceHistoryService)
    {
        _priceHistoryService = priceHistoryService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] ProductPriceHistoryQuery query)
    {
        var result = await _priceHistoryService.GetListAsync(query);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await _priceHistoryService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("current-prices")]
    public async Task<IActionResult> GetCurrentPrices([FromQuery] Guid? branchId = null)
    {
        var result = await _priceHistoryService.GetCurrentPricesAsync(branchId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProductId([FromRoute] Guid productId)
    {
        var result = await _priceHistoryService.GetByProductIdAsync(productId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetByBranchId([FromRoute] Guid branchId)
    {
        var result = await _priceHistoryService.GetByBranchIdAsync(branchId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductPriceHistoryRequest request)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _priceHistoryService.CreateAsync(request, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ProductPriceHistoryRequest request)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _priceHistoryService.UpdateAsync(request, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _priceHistoryService.DeleteAsync(id, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }
}
