using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Chibest.Service.ModelDTOs.Request;

public class ProductRequest
{
    public Guid? Id { get; set; }
    public string Sku { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
    public string? VideoUrl { get; set; }
    public List<Guid>? ColorIds { get; set; }
    public List<Guid>? SizeIds { get; set; }
    public string? Style { get; set; }
    public string? Material { get; set; }
    public int Weight { get; set; }
    public bool IsMaster { get; set; }
    public string Status { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public string? ParentSku { get; set; }
    public IFormFile? AvatarFile { get; set; }
    public IFormFile? VideoFile { get; set; }

    public decimal? SellingPrice { get; set; }
    public decimal? CostPrice { get; set; }

    public string? Note { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}