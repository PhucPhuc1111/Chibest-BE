using Chibest.Common.BusinessResult;
using Chibest.Common.Enums;
using Chibest.Repository.Interface;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Stock.PurchaseOrder;
using Chibest.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Chibest.Service.ModelDTOs.Stock.PurchaseReturn.create;
namespace Chibest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchaseReturn : ControllerBase
    {
        private readonly IPurchaseReturnService _purchaseReturnService;

        public PurchaseReturn(IPurchaseReturnService purchaseReturnService)
        {
            _purchaseReturnService = purchaseReturnService;
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] PurchaseReturnCreate request)
        {
            var result = await _purchaseReturnService.AddPurchaseReturn(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _purchaseReturnService.GetPurchaseReturnById(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetList(
            int pageIndex = 1,
            int pageSize = 10,
            string? search = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? status = null,
            Guid? branchId = null)
        {
            var result = await _purchaseReturnService.GetPurchaseReturnList(pageIndex, pageSize, search, fromDate, toDate, status, branchId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] OrderStatus status)
        {
            var result = await _purchaseReturnService.UpdatePurchaseReturnAsync(id, status);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("import")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PurchaseReturnfile(IFormFile file)
        {
            var result = await _purchaseReturnService.ReadPurchaseReturnFromExcel(file);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePurchaseReturn(Guid id)
        {
            var result = await _purchaseReturnService.DeletePurchaseReturn(id);
            return StatusCode(result.StatusCode, result);
        }

    }
}