using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Mvc;

namespace Chibest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierDebt : ControllerBase
    {
        private readonly ISupplierDebtService _supplierDebtService;

        public SupplierDebt(ISupplierDebtService supplierDebtService)
        {
            _supplierDebtService = supplierDebtService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateSupplierDebt(Guid supplierId,[FromBody] List<SupplierDebtHistoryRequest> transactions)
        {
            var result = await _supplierDebtService.AddSupplierTransactionAsync(supplierId, transactions);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSupplierDebtById(Guid id)
        {
            var result = await _supplierDebtService.GetSupplierDebtAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetSupplierDebtList([FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var result = await _supplierDebtService.GetSupplierDebtList(pageIndex,pageSize,search);
            return StatusCode(result.StatusCode, result);
        }

    }
}
