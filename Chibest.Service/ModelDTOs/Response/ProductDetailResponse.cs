namespace Chibest.Service.ModelDTOs.Response;
public class ProductDetailResponse
{
    public Guid Id { get; set; }

    public string? BarCode { get; set; }
    public string? ChipCode { get; set; }
    public string? TagId { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal PurchasePrice { get; set; }

    public DateTime ImportDate { get; set; }

    public string? LastTransactionType { get; set; }
    public DateTime? LastTransactionDate { get; set; }

    public string? Status { get; set; }

    public string Sku { get; set; }
    public string Name { get; set; }
    public Guid ProductId { get; set; }

    public Guid BranchId { get; set; }

    public Guid? WarehouseId { get; set; }

    public Guid? SupplierId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}