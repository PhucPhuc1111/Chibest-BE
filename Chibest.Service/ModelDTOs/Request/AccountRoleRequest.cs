namespace Chibest.Service.ModelDTOs.Request;
public class AccountRoleRequest
{
    public Guid AccountId { get; set; }

    public Guid RoleId { get; set; }

    public Guid? BranchId { get; set; }
    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
