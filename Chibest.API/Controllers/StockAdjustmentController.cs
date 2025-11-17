using Chibest.API.Attributes;
using Chibest.Common;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Stock.PurchaseOrder;
using Microsoft.AspNetCore.Mvc;
using static Chibest.Service.ModelDTOs.Stock.StockAdjustment.create;
using static Chibest.Service.ModelDTOs.Stock.StockAdjustment.update;

namespace Chibest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Permission(Const.Permissions.StockAdjustment)]
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
        public async Task<IActionResult> GetStockAdjustmentList(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? status = null,
            Guid? branchId = null)
        {
            var result = await _stockAdjusmentService.GetStockAdjustmentList(pageIndex, pageSize, search, fromDate, toDate, status, branchId: branchId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStockAdjustment([FromBody] StockAdjustmentCreate request)
        {
            var result = await _stockAdjusmentService.AddStockAdjustment(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStockAdjustment(Guid id,[FromBody] StockAdjustmentUpdate request)
        {
            var result = await _stockAdjusmentService.UpdateStockAdjustment(id,request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStockAdjustment(Guid id)
        {
            var result = await _stockAdjusmentService.DeleteStockAdjustment(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
