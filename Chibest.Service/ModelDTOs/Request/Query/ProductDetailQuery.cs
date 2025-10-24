namespace Chibest.Service.ModelDTOs.Request.Query;

public class ProductDetailQuery : BaseQuery
{
    public string? ChipCode { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? Status { get; set; }
    public Guid? SupplierId { get; set; }
    public DateTime? ImportDateFrom { get; set; }
    public DateTime? ImportDateTo { get; set; }
}