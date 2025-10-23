using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Stock.PurchaseOrder;
using Microsoft.AspNetCore.Mvc;
using static Chibest.Service.ModelDTOs.Stock.TransferOrder.create;
using static Chibest.Service.ModelDTOs.Stock.TransferOrder.update;

namespace Chibest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferOrder : ControllerBase
    {
        private readonly ITransferOrderService _transferOrderService;

        public TransferOrder(ITransferOrderService transferOrderService)
        {
            _transferOrderService = transferOrderService;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransferOrder(Guid id)
        {
            var result = await _transferOrderService.GetTransferOrderById(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet]
        public async Task<IActionResult> GetTransferOrderList(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null, DateTime? fromDate = null, DateTime? toDate = null, string status = null)
        {
            var result = await _transferOrderService.GetTransferOrderList(pageIndex, pageSize, search, fromDate, toDate, status);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] TransferOrderCreate request)
        {
            var result = await _transferOrderService.AddTransferOrder(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePurchaseOrder(Guid id, [FromBody] TransferOrderUpdate request)
        {
            var result = await _transferOrderService.UpdateTransferOrderAsync(id, request);
            return StatusCode(result.StatusCode, result);
        }

    }
}
