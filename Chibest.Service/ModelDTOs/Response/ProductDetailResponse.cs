namespace Chibest.Service.ModelDTOs.Response;
public class ProductDetailResponse
{
    public Guid Id { get; set; }

    public string? ChipCode { get; set; }

    public decimal PurchasePrice { get; set; }

    public DateTime ImportDate { get; set; }

    public DateTime? LastTransactionDate { get; set; }

    public string? Status { get; set; }

    public Guid ProductId { get; set; }

    public Guid BranchId { get; set; }

    public Guid? WarehouseId { get; set; }

    public Guid? OnlineWarehouseId { get; set; }

    public string? ContainerCode { get; set; }
}