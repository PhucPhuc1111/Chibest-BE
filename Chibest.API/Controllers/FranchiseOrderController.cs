using Chibest.API.Attributes;
using Chibest.Common;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Stock.FranchiseOrder;
using Microsoft.AspNetCore.Mvc;

namespace Chibest.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Permission(Const.Permissions.FranchiseOrder)]
public class FranchiseOrderController : ControllerBase
{
    private readonly IFranchiseService _franchiseService;

    public FranchiseOrderController(IFranchiseService franchiseService)
    {
        _franchiseService = franchiseService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetFranchiseOrder(Guid id)
    {
        var result = await _franchiseService.GetFranchiseOrderByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetFranchiseOrders(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? branchId = null)
    {
        var result = await _franchiseService.GetFranchiseOrderListAsync(pageIndex, pageSize, search, fromDate, toDate, status, branchId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFranchiseOrder([FromBody] FranchiseOrderCreate request)
    {
        var result = await _franchiseService.CreateFranchiseOrderAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateFranchiseOrder(Guid id, [FromBody] FranchiseOrderUpdate request)
    {
        var result = await _franchiseService.UpdateFranchiseOrderAsync(id, request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFranchiseOrder(Guid id)
    {
        var result = await _franchiseService.DeleteFranchiseOrderAsync(id);
        return StatusCode(result.StatusCode, result);
    }

}

