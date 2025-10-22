namespace Chibest.Service.ModelDTOs.Response;
public class RoleResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int AccountCount { get; set; }
}