namespace Chibest.Service.ModelDTOs.Response;

public class ProductResponse
{
    public Guid Id { get; set; }
    public string? AvartarUrl { get; set; }
    public string Sku { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Style { get; set; }
    public string? Brand { get; set; }
    public string? Material { get; set; }
    public int Weight { get; set; }
    public bool IsMaster { get; set; }
    public string Status { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public string? ParentSku { get; set; }

    public decimal? SellingPrice { get; set; }
    public decimal? CostPrice { get; set; }
}
