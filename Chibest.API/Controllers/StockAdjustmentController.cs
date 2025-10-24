using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Stock.PurchaseOrder;
using Microsoft.AspNetCore.Mvc;
using static Chibest.Service.ModelDTOs.Stock.StockAdjustment.create;

namespace Chibest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockAdjustment : ControllerBase
    {
        private readonly IStockAdjusmentService _stockAdjusmentService;

        public StockAdjustment(IStockAdjusmentService stockAdjusmentService)
        {
            _stockAdjusmentService = stockAdjusmentService;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStockAdjustment(Guid id)
        {
            var result = await _stockAdjusmentService.GetStockAdjustmentById(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrderList(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null, DateTime? fromDate = null, DateTime? toDate = null, string status = null)
        {
            var result = await _stockAdjusmentService.GetStockAdjustmentList(pageIndex, pageSize, search, fromDate, toDate, status);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] StockAdjustmentCreate request)
        {
            var result = await _stockAdjusmentService.AddStockAdjustment(request);
            return StatusCode(result.StatusCode, result);
        }
    }
}
