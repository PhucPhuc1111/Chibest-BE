using Chibest.API.Attributes;
using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Stock.PurchaseOrder;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using static Chibest.Service.ModelDTOs.Stock.TransferOrder.create;
using static Chibest.Service.ModelDTOs.Stock.TransferOrder.update;

namespace Chibest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Permission(Const.Permissions.TransferOrder)]
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
            [FromQuery] string? search = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? status = null,
            Guid? fromBranchId = null,
            Guid? toBranchId = null,
            Guid? branchId = null)
        {
            var result = await _transferOrderService.GetTransferOrderList(pageIndex, pageSize, search, fromDate, toDate, status, fromBranchId, toBranchId, branchId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransferOrder([FromBody] TransferOrderCreate request)
        {
            var result = await _transferOrderService.AddTransferOrder(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("multiple")]
        public async Task<IActionResult> CreateMultipleTransferOrder([FromBody] TransferMultiOrderCreate request)
        {
            var result = await _transferOrderService.AddMultiTransferOrder(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransferOrder(Guid id, [FromBody] TransferOrderUpdate request)
        {
            var result = await _transferOrderService.UpdateTransferOrderAsync(id, request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("import")]
        public async Task<IActionResult> TransferOrderfile(IFormFile file)
        {
            var result = await _transferOrderService.ReadTransferDetailFromExcel(file);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransferOrder(Guid id)
        {
            var result = await _transferOrderService.DeleteTransferOrder(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}