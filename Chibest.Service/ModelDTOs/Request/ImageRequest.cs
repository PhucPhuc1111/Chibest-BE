using Microsoft.AspNetCore.Http;

namespace Chibest.Service.ModelDTOs.Request;

public class ImageRequest
{
    public required IFormFile FileData { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
}
