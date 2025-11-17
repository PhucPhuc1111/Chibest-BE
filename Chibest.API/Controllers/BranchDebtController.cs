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
    [Permission(Const.Permissions.BranchDebt)]
    public class BranchDebt : ControllerBase
    {
        private readonly IBranchDebtService _branchDebtService;

        public BranchDebt(IBranchDebtService branchDebtService)
        {
            _branchDebtService = branchDebtService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateBranchDebt(Guid branchDebtId, [FromBody] List<BranchDebtHistoryRequest> transactions)
        {
            var result = await _branchDebtService.AddBranchTransactionAsync(branchDebtId, transactions);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBranchDebtById(Guid id, string transactionType = null)
        {
            var result = await _branchDebtService.GetBranchDebtAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetBranchDebtList(
    [FromQuery] int pageIndex = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? search = null,
    [FromQuery] decimal? totalFrom = null,
    [FromQuery] decimal? totalTo = null,
    [FromQuery] string? datePreset = null,
    [FromQuery] DateTime? fromDate = null,
    [FromQuery] DateTime? toDate = null,
    [FromQuery] decimal? debtFrom = null,
    [FromQuery] decimal? debtTo = null)
        {
            var result = await _branchDebtService.GetBranchDebtList(
                pageIndex,
                pageSize,
                search,
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
        public async Task<IActionResult> DeleteBranchDebt(Guid branchdebtId, Guid historyId)
        {
            var result = await _branchDebtService.DeleteBranchDebtHistoryAsync(branchdebtId, historyId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportBranchDebt()
        {
            var fileBytes = await _branchDebtService.ExportBranchDebtToExcelAsync();
            var fileName = $"BranchDebts_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileBytes, MediaTypeNames.Application.Octet, fileName);
        }

    }
}
