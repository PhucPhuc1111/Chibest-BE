namespace Chibest.Service.ModelDTOs.Request;
public class BranchStockRequest
{
    public Guid? Id { get; set; }
    public int AvailableQty { get; set; } = 0;
    public int MinimumStock { get; set; } = 0;
    public int MaximumStock { get; set; } = 0;
    public Guid ProductId { get; set; }

    public Guid BranchId { get; set; }
}