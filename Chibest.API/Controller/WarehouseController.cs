using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chibest.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetWarehouseList(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = null)
        {
            var result = await _warehouseService.GetWarehouseList(pageIndex, pageSize, search);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWarehouseByID(Guid id)
        {
            var result = await _warehouseService.GetWarehouseById(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWarehouse([FromBody] WarehouseRequest request)
        {
            var result = await _warehouseService.CreateWarehouse(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWarehouse(Guid id, [FromBody] WarehouseRequest request)
        {
            var result = await _warehouseService.UpdateWarehouse(id, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarehouse(Guid id)
        {
            var result = await _warehouseService.DeleteWarehouse(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
