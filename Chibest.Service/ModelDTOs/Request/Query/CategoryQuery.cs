namespace Chibest.Service.ModelDTOs.Request.Query;

public class CategoryQuery : BaseQuery
{
    public string? Type { get; set; }
    public string? Name { get; set; }
    public Guid? ParentId { get; set; }
    public bool? OnlyRoot { get; set; }
}