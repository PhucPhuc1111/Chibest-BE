using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Mvc;

namespace Chibest.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductDetailController : ControllerBase
{
    private readonly IProductDetailService _productDetailService;

    public ProductDetailController(IProductDetailService productDetailService)
    {
        _productDetailService = productDetailService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
        [FromQuery] Guid? productId = null, [FromQuery] Guid? branchId = null, [FromQuery] string? status = null)
    {
        var result = await _productDetailService.GetPagedAsync(pageNumber, pageSize, productId, branchId, status);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await _productDetailService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("product/{productId}/branch/{branchId}")]
    public async Task<IActionResult> GetByProductAndBranch([FromRoute] Guid productId, [FromRoute] Guid branchId)
    {
        var result = await _productDetailService.GetByProductAndBranchAsync(productId, branchId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductDetailRequest request)
    {
        var result = await _productDetailService.CreateAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] ProductDetailRequest request)
    {
        var result = await _productDetailService.UpdateAsync(id, request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await _productDetailService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}
