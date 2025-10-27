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
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] CategoryQuery query)
    {
        var result = await _categoryService.GetListAsync(query);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("type/{type}")]
    public async Task<IActionResult> GetByType(string type)
    {
        var result = await _categoryService.GetByTypeAsync(type);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("hierarchy")]
    public async Task<IActionResult> GetHierarchy()
    {
        var result = await _categoryService.GetHierarchyAsync();
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("parent/{parentId}/children")]
    public async Task<IActionResult> GetChildren(Guid parentId)
    {
        var result = await _categoryService.GetChildrenAsync(parentId);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryRequest request)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _categoryService.CreateAsync(request, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] CategoryRequest request)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _categoryService.UpdateAsync(request, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _categoryService.DeleteAsync(id, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }
}