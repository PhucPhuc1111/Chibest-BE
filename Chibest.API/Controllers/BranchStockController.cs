using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chibest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchStockController : ControllerBase
{
    private readonly IBranchStockService _branchStockService;

    public BranchStockController(IBranchStockService branchStockService)
    {
        _branchStockService = branchStockService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
        [FromQuery] Guid? productId = null, [FromQuery] Guid? branchId = null)
    {
        var result = await _branchStockService.GetPagedAsync(pageNumber, pageSize, productId, branchId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await _branchStockService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("product/{productId}/branch/{branchId}")]
    public async Task<IActionResult> GetByProductAndBranch([FromRoute] Guid productId, [FromRoute] Guid branchId)
    {
        var result = await _branchStockService.GetByProductAndBranchAsync(productId, branchId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("low-stock/{branchId}")]
    public async Task<IActionResult> GetLowStockItems([FromRoute] Guid branchId)
    {
        var result = await _branchStockService.GetLowStockItemsAsync(branchId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BranchStockRequest request)
    {
        var result = await _branchStockService.CreateAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] BranchStockRequest request)
    {
        var result = await _branchStockService.UpdateAsync(id, request);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPatch("{id}/stock")]
    public async Task<IActionResult> UpdateStock([FromRoute] Guid id, [FromBody] UpdateStockRequest request)
    {
        var result = await _branchStockService.UpdateStockAsync(id, request.AvailableQty, request.ReservedQty);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await _branchStockService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}