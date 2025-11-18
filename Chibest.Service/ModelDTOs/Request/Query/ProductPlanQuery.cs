namespace Chibest.Service.ModelDTOs.Request.Query;

public class ProductPlanQuery
{
    public Guid? ProductId { get; set; }
    public Guid? SupplierId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromSendDate { get; set; }
    public DateTime? ToSendDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

