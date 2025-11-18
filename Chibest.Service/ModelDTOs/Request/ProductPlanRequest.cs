namespace Chibest.Service.ModelDTOs.Request;

public class ProductPlanRequest
{
    public Guid? Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? SupplierId { get; set; }
    public string? Type { get; set; }
    public DateTime? SendDate { get; set; }
    public string? DetailAmount { get; set; }
    public int? Amount { get; set; }
    public string Status { get; set; } = "Queue";
    public string? Note { get; set; }
}

