using Microsoft.AspNetCore.Http;
using System;

namespace Chibest.Service.ModelDTOs.Request;

public class ProductUpdateRequest
{
    public IFormFile? AvatarFile { get; set; }
    public IFormFile? VideoFile { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
}

