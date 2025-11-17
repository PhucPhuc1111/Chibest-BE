using Chibest.Common;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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
    [HttpGet("master")]
    public async Task<IActionResult> GetMasterList([FromQuery] ProductQuery query)
    {
        var result = await _productService.GetMasterListAsync(query);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] ProductQuery query)
    {
        var result = await _productService.GetListAsync(query);
        return StatusCode(result.StatusCode, result);
    }

    //[HttpPost("import")]
    //public async Task<IActionResult> ImportProducts( IFormFile file)
    //{
    //    if (file == null)
    //        return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, "File không hợp lệ.");

    //    var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //    if (string.IsNullOrEmpty(accountId) || accountId == Guid.Empty.ToString())
    //        return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

    //    var result = await _productService.ImportProductsFromExcelAsync(file, Guid.Parse(accountId));
    //    return StatusCode(result.StatusCode, result);
    //}

    [HttpGet("{id}/{branchId}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, [FromRoute] Guid? branchId)
    {
        var result = await _productService.GetByIdAsync(id,branchId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("sku/{sku}/{branchId}")]
    public async Task<IActionResult> GetBySKU([FromRoute] string sku, [FromRoute] Guid? branchId)
    {
        var result = await _productService.GetVariantsByParentSkuAsync(sku,branchId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductRequest request)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (accountId == null || accountId == Guid.Empty.ToString())
            return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var result = await _productService.CreateAsync(request, Guid.Parse(accountId));
        return StatusCode(result.StatusCode, result);
    }

    //[HttpPut]
    //public async Task<IActionResult> Update([FromBody] ProductRequest request)
    //{
    //    var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //    if (accountId == null || accountId == Guid.Empty.ToString())
    //        return StatusCode(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

    //    var result = await _productService.UpdateAsync(request, Guid.Parse(accountId));
    //    return StatusCode(result.StatusCode, result);
    //}

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