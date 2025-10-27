namespace Chibest.Service.ModelDTOs.Response;
public class BranchStockResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid BranchId { get; set; }
    public Guid? WarehouseId { get; set; }
    public int AvailableQty { get; set; }
    public int ReservedQty { get; set; }
    public int InTransitQty { get; set; }
    public int DefectiveQty { get; set; }
    public int TotalQty { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
    public int ReorderPoint { get; set; }
    public int ReorderQty { get; set; }
    public decimal CurrentSellingPrice { get; set; }
    public DateTime LastUpdated { get; set; }

    // Navigation properties
    public string? ProductName { get; set; }
    public string? ProductSKU { get; set; }
    public string? BranchName { get; set; }
    public string? WarehouseName { get; set; }
}