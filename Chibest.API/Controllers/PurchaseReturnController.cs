using Chibest.Common.BusinessResult;
using Chibest.Common.Enums;
using Chibest.Repository.Interface;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Stock.PurchaseOrder;
using Chibest.Service.Services;
using Microsoft.AspNetCore.Mvc;
using static Chibest.Service.ModelDTOs.Stock.PurchaseReturn.create;
namespace Chibest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseReturn : ControllerBase
    {
        private readonly IPurchaseReturnService _purchaseReturnService;

        public PurchaseReturn(IPurchaseReturnService purchaseReturnService)
        {
            _purchaseReturnService = purchaseReturnService;
        }
        [HttpPost]
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
            string search = "",
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string status = null)
        {
            var result = await _purchaseReturnService.GetPurchaseReturnList(pageIndex, pageSize, search, fromDate, toDate, status);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] OrderStatus status)
        {
            var result = await _purchaseReturnService.UpdatePurchaseReturnAsync(id, status);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("import")]
        public async Task<IActionResult> PurchaseReturnfile(IFormFile file)
        {
            var result = await _purchaseReturnService.ReadPurchaseReturnFromExcel(file);
            return StatusCode(result.StatusCode, result);
        }
    }
}