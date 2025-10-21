using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;

namespace Chibest.Service.Interface;

public interface ISystemLogService
{
    Task<IBusinessResult> GetByIdAsync(Guid id);
    Task<IBusinessResult> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        string? logLevel = null,
        string? module = null);
    Task<IBusinessResult> CreateAsync(SystemLogRequest request);
    Task<IBusinessResult> DeleteAsync(Guid id);
    Task<IBusinessResult> DeleteOldLogsAsync(DateTime olderThan);
}