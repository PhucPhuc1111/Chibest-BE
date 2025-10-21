using Chibest.Repository.Models;

namespace Chibest.Service.ModelDTOs.Response;

public class CategoryResponse
{
    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
    public virtual ICollection<Product>? Products { get; set; }
}