namespace Chibest.Service.ModelDTOs.Request;

public class RolePermissionRequest
{
    public Guid RoleId { get; set; }
    public List<Guid> PermissionIds { get; set; } = new List<Guid>();
}

