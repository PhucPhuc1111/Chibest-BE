using Chibest.API.Attributes;
using Chibest.Common;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Stock.PurchaseOrder;
using Microsoft.AspNetCore.Mvc;

namespace Chibest.API.Controllers;

[Route("api/purchase-invoices")]
[ApiController]
[Permission(Const.Permissions.PurchaseOrder)]
public class PurchaseInvoiceController : ControllerBase
{
    private readonly IPurchaseInvoiceService _service;

    public PurchaseInvoiceController(IPurchaseInvoiceService service)
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
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? supplierId = null)
    {
        var result = await _service.GetInvoiceListAsync(pageIndex, pageSize, search, fromDate, toDate, status, branchId, supplierId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetInvoice(Guid id)
    {
        var result = await _service.GetInvoiceByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateInvoice([FromBody] PurchaseInvoiceCreate request)
    {
        var result = await _service.CreateInvoiceAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateInvoiceStatus(Guid id, [FromBody] PurchaseInvoiceStatusUpdate request)
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

