namespace Chibest.Service.ModelDTOs.Response;
public class RoleResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int AccountCount { get; set; }
    public List<PermissionResponse> Permissions { get; set; } = new List<PermissionResponse>();
}