using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Mvc;

namespace Chibest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchDebt : ControllerBase
    {
        private readonly IBranchDebtService _branchDebtService;

        public BranchDebt(IBranchDebtService branchDebtService)
        {
            _branchDebtService = branchDebtService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateBranchDebt(Guid branchId, [FromBody] List<BranchDebtHistoryRequest> transactions)
        {
            var result = await _branchDebtService.AddBranchTransactionAsync(branchId, transactions);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBranchDebtById(Guid id)
        {
            var result = await _branchDebtService.GetBranchDebtAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetBranchDebtList([FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var result = await _branchDebtService.GetBranchDebtList(pageIndex, pageSize, search);
            return StatusCode(result.StatusCode, result);
        }

    }
}
