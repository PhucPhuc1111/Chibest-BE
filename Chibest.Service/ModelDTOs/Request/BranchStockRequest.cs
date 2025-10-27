namespace Chibest.Service.ModelDTOs.Request;
public class BranchStockRequest
{
    public Guid? Id { get; set; }
    public decimal CurrentSellingPrice { get; set; } = 0;
    public int AvailableQty { get; set; } = 0;
    public int ReservedQty { get; set; } = 0;
    public int MinimumStock { get; set; } = 0;
    public int MaximumStock { get; set; } = 0;
    public int InTransitQty { get; set; } = 0;
    public int DefectiveQty { get; set; } = 0;
    public int ReorderPoint { get; set; } = 0;
    public int ReorderQty { get; set; } = 0;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    public Guid ProductId { get; set; }

    public Guid BranchId { get; set; }

    public Guid? WarehouseId { get; set; }
}