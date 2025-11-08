namespace Chibest.Service.ModelDTOs.Response;
public class ProductPriceHistoryResponse
{
    public Guid Id { get; set; }

    public decimal SellingPrice { get; set; }

    public decimal CostPrice { get; set; }

    public DateTime EffectiveDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid ProductId { get; set; }

    public Guid? BranchId { get; set; }
    public string Sku { get; set; } = null!;

    public string Name { get; set; } = null!;
}