namespace Chibest.Service.ModelDTOs.Response;

public class RoleWithAccountsResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<AccountRoleResponse> AccountRoles { get; set; } = new();
}