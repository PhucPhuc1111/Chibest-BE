namespace Chibest.Service.ModelDTOs.Request;
public class ProductDetailRequest
{
    public Guid Id { get; set; }

    public string? ChipCode { get; set; }

    public Guid ProductId { get; set; }

    public Guid BranchId { get; set; }

    public Guid? WarehouseId { get; set; }

    public decimal PurchasePrice { get; set; }

    public DateTime ImportDate { get; set; }

    public Guid? SupplierId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? LastTransactionDate { get; set; }

    public string? LastTransactionType { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}