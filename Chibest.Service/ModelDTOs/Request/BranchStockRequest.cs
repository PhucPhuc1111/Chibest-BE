namespace Chibest.Service.ModelDTOs.Request;
public class BranchStockRequest
{
    public Guid Id { get; set; }

    public decimal SellingPrice { get; set; }

    public int ReOrderPriority { get; set; }

    public int AvailableQty { get; set; }

    public int ReservedQty { get; set; }

    public int MinimumStock { get; set; }

    public DateTime LastUpdated { get; set; }

    public Guid ProductId { get; set; }

    public Guid BranchId { get; set; }

    public Guid? WarehouseId { get; set; }

    public Guid? OnlineWarehouseId { get; set; }
}