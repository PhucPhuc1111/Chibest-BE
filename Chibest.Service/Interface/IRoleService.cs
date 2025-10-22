using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;

namespace Chibest.Service.Interface;

public interface IRoleService
{
    Task<IBusinessResult> GetByIdAsync(Guid id);
    Task<IBusinessResult> GetPagedAsync(int pageNumber, int pageSize, string? search = null);
    Task<IBusinessResult> CreateAsync(RoleRequest request);
    Task<IBusinessResult> UpdateAsync(RoleRequest request);
    Task<IBusinessResult> ChangeRoleAccountAsync(Guid accountId, Guid roleId);
    Task<IBusinessResult> DeleteAsync(Guid id);
    Task<IBusinessResult> GetRoleWithAccountsAsync(Guid id);
    Task<IBusinessResult> GetAllRolesAsync();
}