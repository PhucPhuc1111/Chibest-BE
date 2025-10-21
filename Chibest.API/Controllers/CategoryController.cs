using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? type = null)
    {
        var result = await _categoryService.GetPagedAsync(pageNumber, pageSize, search, type);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("with-products")]
    public async Task<IActionResult> GetWithProducts()
    {
        var result = await _categoryService.GetCategoriesWithProductsAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryRequest request)
    {
        var result = await _categoryService.CreateAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryRequest request)
    {
        var result = await _categoryService.UpdateAsync(id, request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _categoryService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}