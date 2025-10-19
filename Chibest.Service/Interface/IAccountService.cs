using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;

namespace Chibest.Service.Interface;

public interface IAccountService
{
    Task<IBusinessResult> LoginByPasswordAsync(AuthRequest authRequest);
    Task<IBusinessResult> RefreshTokenAsync(AuthTokenRequest request);
    Task RevokeRefreshTokenAsync(Guid accountId);
    //============================================================
    Task<IBusinessResult> GetAccountByIdAsync(Guid id);
    Task<IBusinessResult> GetAccountsListAsync(int pageIndex, int pageSize, string? search);
    Task<IBusinessResult> CreateAccountAsync(AccountRequest request);
    Task<IBusinessResult> UpdateAccountAsync(AccountRequest newData);
    Task<IBusinessResult> ChangeAccountStatusAsync(Guid accountId, string status);
    Task<IBusinessResult> ChangeAccountPasswordAsync(Guid accountId, string newPassword);
    Task<IBusinessResult> DeleteAccountAsync(Guid id);
}