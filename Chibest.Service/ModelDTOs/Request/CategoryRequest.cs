namespace Chibest.Service.ModelDTOs.Request;
public class CategoryRequest
{
    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}