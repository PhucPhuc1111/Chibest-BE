using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] Guid? categoryId = null, [FromQuery] string? status = null)
    {
        var result = await _productService.GetPagedAsync(pageNumber, pageSize, search, categoryId, status);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await _productService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductRequest request)
    {
        var result = await _productService.CreateAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute]Guid id, [FromBody] ProductRequest request)
    {
        var result = await _productService.UpdateAsync(id, request);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] string status)
    {
        var result = await _productService.UpdateStatusAsync(id, status);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await _productService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}