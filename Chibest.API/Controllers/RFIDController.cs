using Chibest.API.Attributes;
using Chibest.Common;
using Microsoft.AspNetCore.Mvc;

namespace Chibest.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RFIDController : ControllerBase
    {
        [HttpGet]
        public IActionResult get(string rfid)
        {
            return Ok(rfid);
        }
    }
}
