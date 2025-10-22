namespace Chibest.Service.ModelDTOs.Response;

public class AccountRoleResponse
{
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = null!;
    public string AccountEmail { get; set; } = null!;
}