using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Response;
using Chibest.Service.Utilities;
using Mapster;
using Microsoft.EntityFrameworkCore;


namespace Chibest.Service.Services;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public AccountService(IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    //=============================================================================
    public async Task<IBusinessResult> GetAccountByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var account = await _unitOfWork.AccountRepository.GetByIdAsync(id);
        if (account == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var response = account.Adapt<AccountResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> LoginByPasswordAsync(AuthRequest authRequest)
    {
        if (BoolUtils.IsValidEmail(authRequest.Email) == false)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Check account in database
        var existAccount = await _unitOfWork.AccountRepository.GetByWhere(
            acc => acc.Email.ToLower().Equals(authRequest.Email.ToLower()))
            .FirstOrDefaultAsync();

        if (existAccount is null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        //Find Role
        var existAccRole = await _unitOfWork.AccountRoleRepository.GetByWhere(
            ar => ar.AccountId == existAccount.Id &&
            (ar.EndDate == null || ar.EndDate > DateTime.Now) &&
            ar.BranchId == authRequest.BranchId)
            .Include(accR => accR.Role)
            .FirstOrDefaultAsync();
        if (existAccRole is null)
            return new BusinessResult(Const.HTTP_STATUS_FORBIDDEN, "Tài khoản không có quyền truy cập chi nhánh này hoặc đã hết hạn!");

        //Hash Password
        var securedPassword = StringUtils.HashStringSHA256(authRequest.Password);

        // If password not match
        if (!securedPassword.Equals(existAccount.Password))
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.FAIL_LOGIN_MSG);

        //Generate Tokens
        var accessToken = _tokenService.GenerateAccessToken(existAccount, existAccRole.Role.Name);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Lưu refresh token vào database
        existAccount.RefreshToken = refreshToken;
        existAccount.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
        existAccount.UpdatedAt = DateTime.Now;

        await _unitOfWork.SaveChangesAsync();
        var authResponse = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 15 * 60,//default as setting, go to JWT settings to edit
            AccountId = existAccount.Id,
            Email = existAccount.Email,
            UserName = existAccount.Name,
            Role = existAccRole.Role.Name
        };
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_LOGIN_MSG, authResponse);
    }

}