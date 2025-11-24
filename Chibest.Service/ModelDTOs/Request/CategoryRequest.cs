namespace Chibest.Service.ModelDTOs.Request;
public class CategoryRequest
{
    public Guid? Id { get; set; }

    public string Name { get; set; } = null!;
}