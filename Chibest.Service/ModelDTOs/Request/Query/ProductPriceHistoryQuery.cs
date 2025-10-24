namespace Chibest.Service.ModelDTOs.Request.Query;

public class ProductPriceHistoryQuery : BaseQuery
{
    public Guid? ProductId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? EffectiveDateFrom { get; set; }
    public DateTime? EffectiveDateTo { get; set; }
    public DateTime? ExpiryDateFrom { get; set; }
    public DateTime? ExpiryDateTo { get; set; }
    public bool? IsActive { get; set; }
    public string? Note { get; set; }
}