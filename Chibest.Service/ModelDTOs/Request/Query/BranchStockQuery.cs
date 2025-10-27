namespace Chibest.Service.ModelDTOs.Request.Query;
public class BranchStockQuery : BaseQuery
{
    public Guid? ProductId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? WarehouseId { get; set; }
    public int? MinAvailableQty { get; set; }
    public int? MaxAvailableQty { get; set; }
    public bool? IsLowStock { get; set; }
    public bool? IsOutOfStock { get; set; }
    public bool? NeedsReorder { get; set; }
}