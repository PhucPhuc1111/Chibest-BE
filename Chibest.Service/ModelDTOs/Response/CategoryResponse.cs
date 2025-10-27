using Chibest.Repository.Models;

namespace Chibest.Service.ModelDTOs.Response;

public class CategoryResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }

    // Navigation properties
    public string? ParentName { get; set; }
    public int ProductCount { get; set; }
    public List<CategoryResponse>? Children { get; set; }
}