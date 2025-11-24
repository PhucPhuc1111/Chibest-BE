namespace Chibest.Service.ModelDTOs.Request;

public class PermissionRequest
{
    public Guid? Id { get; set; }
    public string Code { get; set; } = null!;
}

