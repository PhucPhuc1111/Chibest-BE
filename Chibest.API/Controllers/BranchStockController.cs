﻿using Chibest.Common;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    public async Task<IActionResult> GetList([FromQuery] BranchStockQuery query)
    {
        var result = await _branchStockService.GetListAsync(query);
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
    public async Task<IActionResult> GetByProductAndBranch([FromRoute] Guid productId, [FromRoute] Guid branchId,
        [FromQuery] Guid? warehouseId = null)
    {
        var result = await _branchStockService.GetByProductAndBranchAsync(productId, branchId, warehouseId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockItems([FromQuery] Guid? branchId = null)
    {
        var result = await _branchStockService.GetLowStockItemsAsync(branchId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("need-reorder")]
    public async Task<IActionResult> GetItemsNeedingReorder([FromQuery] Guid? branchId = null)
    {
        var result = await _branchStockService.GetItemsNeedingReorderAsync(branchId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BranchStockRequest request)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _branchStockService.CreateAsync(request, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] BranchStockRequest request)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _branchStockService.UpdateAsync(request, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _branchStockService.DeleteAsync(id, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }
}