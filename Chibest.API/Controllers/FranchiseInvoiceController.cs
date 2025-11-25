using Chibest.API.Attributes;
using Chibest.Common;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Stock.FranchiseOrder;
using Microsoft.AspNetCore.Mvc;

namespace Chibest.API.Controllers;

[Route("api/franchise-invoices")]
[ApiController]
[Permission(Const.Permissions.FranchiseOrder)]
public class FranchiseInvoiceController : ControllerBase
{
    private readonly IFranchiseInvoiceService _service;

    public FranchiseInvoiceController(IFranchiseInvoiceService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? branchId = null)
    {
        var result = await _service.GetInvoiceListAsync(pageIndex, pageSize, search, fromDate, toDate, status, branchId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetInvoice(Guid id)
    {
        var result = await _service.GetInvoiceByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateInvoice([FromBody] FranchiseInvoiceCreate request)
    {
        var result = await _service.CreateInvoiceAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateInvoiceStatus(Guid id, [FromBody] FranchiseInvoiceStatusUpdate request)
    {
        var result = await _service.UpdateInvoiceStatusAsync(id, request.Status);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteInvoice(Guid id)
    {
        var result = await _service.DeleteInvoiceAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}

