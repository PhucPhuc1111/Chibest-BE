using Chibest.Repository.Models;

namespace Chibest.Service.ModelDTOs.Response;

public class CategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}