using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;

namespace Chibest.Service.Interface;

public interface IRoleService
{
    Task<IBusinessResult> GetByIdAsync(Guid id);
    Task<IBusinessResult> GetPagedAsync(int pageNumber, int pageSize, string? search = null);
    Task<IBusinessResult> GetRoleWithAccountsAsync(Guid id);
    Task<IBusinessResult> GetAllRolesAsync();
    Task<IBusinessResult> CreateAsync(RoleRequest request, Guid accountId);
    Task<IBusinessResult> CreateAccountRoleAsync(AccountRoleRequest request, Guid makerId);
    Task<IBusinessResult> UpdateAsync(RoleRequest request, Guid accountId);
    Task<IBusinessResult> ChangeRoleAccountAsync(AccountRoleRequest request, Guid whoMakeId);
    Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId);
    Task<IBusinessResult> DeleteAccountRoleAsync(Guid accountId, Guid roleId, Guid makerId);
    
    // Permission management methods
    Task<IBusinessResult> GetRolePermissionsAsync(Guid roleId);
    Task<IBusinessResult> AssignPermissionsToRoleAsync(RolePermissionRequest request, Guid accountId);
    Task<IBusinessResult> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, Guid accountId);
    Task<IBusinessResult> UpdateRolePermissionsAsync(RolePermissionRequest request, Guid accountId);
}