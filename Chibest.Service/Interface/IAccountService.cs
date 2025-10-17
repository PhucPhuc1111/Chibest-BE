using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;

namespace Chibest.Service.Interface;

public interface IAccountService
{
    Task<IBusinessResult> GetAccountByIdAsync(Guid id);
    Task<IBusinessResult> LoginByPasswordAsync(AuthRequest authRequest);
}