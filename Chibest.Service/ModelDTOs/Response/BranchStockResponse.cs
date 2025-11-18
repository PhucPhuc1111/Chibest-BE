namespace Chibest.Service.ModelDTOs.Response;
public class BranchStockResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid BranchId { get; set; }
    public int AvailableQty { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }

    // Navigation properties
    public string? ProductName { get; set; }
    public string? ProductSKU { get; set; }
    public string? BranchName { get; set; }
}