namespace Chibest.Service.ModelDTOs.Response;

public class ProductPlanResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? SupplierId { get; set; }
    public string? Type { get; set; }
    public DateTime SendDate { get; set; }
    public string? DetailAmount { get; set; }
    public int? Amount { get; set; }
    public string Status { get; set; } = null!;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? ProductName { get; set; }
    public string? SupplierName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? VideoUrl { get; set; }

}

