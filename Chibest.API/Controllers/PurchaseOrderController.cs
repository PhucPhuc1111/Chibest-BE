using Azure.Core;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Stock.PurchaseOrder;
using Microsoft.AspNetCore.Mvc;

namespace Chibest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrder : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrder(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPurchaseOrder(Guid id)
        {
            var result = await _purchaseOrderService.GetPurchaseOrderById(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrderList(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null, DateTime? fromDate = null, DateTime? toDate = null, string status = null)
        {
            var result = await _purchaseOrderService.GetPurchaseOrderList(pageIndex, pageSize, search, fromDate,toDate,status);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] PurchaseOrderCreate request)
        {
            var result = await _purchaseOrderService.AddPurchaseOrder(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePurchaseOrder(Guid id,[FromBody] PurchaseOrderUpdate request)
        {
            var result = await _purchaseOrderService.UpdateAsync(id,request);
            return StatusCode(result.StatusCode, result);
        }
    }
}
