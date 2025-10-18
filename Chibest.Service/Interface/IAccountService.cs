using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;

namespace Chibest.Service.Interface;

public interface IAccountService
{
    Task<IBusinessResult> LoginByPasswordAsync(AuthRequest authRequest);
    Task<IBusinessResult> RefreshTokenAsync(AuthTokenRequest request);
    Task RevokeRefreshTokenAsync(Guid accountId);
    Task<IBusinessResult> GetAccountByIdAsync(Guid id);
}