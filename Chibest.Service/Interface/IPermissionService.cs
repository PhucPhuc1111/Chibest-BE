using System;
using System.Threading.Tasks;
using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;

namespace Chibest.Service.Interface;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid accountId, params string[] permissionCodes);
    Task<IBusinessResult> GetByIdAsync(Guid id);
    Task<IBusinessResult> GetAllAsync();
    Task<IBusinessResult> GetPagedAsync(int pageNumber, int pageSize, string? search = null);
    Task<IBusinessResult> CreateAsync(PermissionRequest request, Guid accountId);
    Task<IBusinessResult> UpdateAsync(PermissionRequest request, Guid accountId);
    Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId);
}

