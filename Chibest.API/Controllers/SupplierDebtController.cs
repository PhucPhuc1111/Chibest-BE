using Chibest.API.Attributes;
using Chibest.Common;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Mime;

namespace Chibest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Permission(Const.Permissions.SupplierDebt)]
    public class SupplierDebt : ControllerBase
    {
        private readonly ISupplierDebtService _supplierDebtService;

        public SupplierDebt(ISupplierDebtService supplierDebtService)
        {
            _supplierDebtService = supplierDebtService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateSupplierDebt(Guid supplierDebtId,[FromForm] List<SupplierDebtHistoryRequest> transactions)
        {
            var result = await _supplierDebtService.AddSupplierTransactionAsync(supplierDebtId, transactions);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSupplierDebtById(Guid id,[FromQuery] string transactionType = "all")
        {
            var result = await _supplierDebtService.GetSupplierDebtAsync(id,transactionType);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetSupplierDebtList(
    [FromQuery] int pageIndex = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? q = null,
    [FromQuery] decimal? totalFrom = null,
    [FromQuery] decimal? totalTo = null,
    [FromQuery] string? datePreset = null,
    [FromQuery] DateTime? fromDate = null,
    [FromQuery] DateTime? toDate = null,
    [FromQuery] decimal? debtFrom = null,
    [FromQuery] decimal? debtTo = null)
        {
            var result = await _supplierDebtService.GetSupplierDebtList(
                pageIndex,
                pageSize,
                q,
                totalFrom,
                totalTo,
                datePreset,
                fromDate,
                toDate,
                debtFrom,
                debtTo
            );

            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSupplierDebt(Guid supplierdebtId, Guid historyId)
        {
            var result = await _supplierDebtService.DeleteSupplierDebtHistoryAsync(supplierdebtId,historyId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{supplierdebtId}/histories/{historyId}")]
        public async Task<IActionResult> UpdateSupplierDebtHistory(Guid supplierdebtId, Guid historyId, [FromForm] SupplierDebtHistoryUpdateRequest request)
        {
            var result = await _supplierDebtService.UpdateSupplierDebtHistoryAsync(supplierdebtId, historyId, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportSupplierDebt()
        {
            var fileBytes = await _supplierDebtService.ExportSupplierDebtToExcelAsync();
            var fileName = $"SupplierDebts_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileBytes, MediaTypeNames.Application.Octet, fileName);
        }

    }
}
