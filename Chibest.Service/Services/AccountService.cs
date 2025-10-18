using Azure.Core;
using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Response;
using Chibest.Service.Utilities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;


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

    //=================================================================================================
    //Authentication
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
            (ar.EndDate == null || ar.EndDate > DateTime.Now))
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
        var expireTimes = _tokenService.GetExpirationTimes();

        // Lưu refresh token vào database
        existAccount.RefreshToken = refreshToken;
        existAccount.RefreshTokenExpiryTime = DateTime.Now.AddDays(expireTimes.refreshDay);
        existAccount.UpdatedAt = DateTime.Now;

        await _unitOfWork.SaveChangesAsync();
        var authResponse = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expireTimes.accessMinute * 60,//convert to seconds
            AccountId = existAccount.Id,
            Email = existAccount.Email,
            UserName = existAccount.Name,
            Role = existAccRole.Role.Name
        };
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_LOGIN_MSG, authResponse);
    }

    public async Task<IBusinessResult> RefreshTokenAsync(AuthTokenRequest request)
    {
        //Decode token to get accountId
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var accountId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        // Get accountId from token
        if (accountId == null || accountId == Guid.Empty.ToString())
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Get account from accountId
        var existAccount = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
        if (existAccount == null ||
            existAccount.RefreshToken != request.RefreshToken ||
            existAccount.RefreshTokenExpiryTime <= DateTime.Now)
        {
            throw new SecurityTokenException("Invalid refresh token");
        }

        //Find Role
        var existAccRole = await _unitOfWork.AccountRoleRepository.GetByWhere(
            ar => ar.AccountId == existAccount.Id &&
            (ar.EndDate == null || ar.EndDate > DateTime.Now))
            .Include(accR => accR.Role)
            .FirstOrDefaultAsync();
        if (existAccRole is null)
            return new BusinessResult(Const.HTTP_STATUS_FORBIDDEN, "Tài khoản không có quyền truy cập chi nhánh này hoặc đã hết hạn!");

        var newAccessToken = _tokenService.GenerateAccessToken(existAccount, existAccRole.Role.Name);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var expireTimes = _tokenService.GetExpirationTimes();

        existAccount.RefreshToken = newRefreshToken;
        existAccount.RefreshTokenExpiryTime = DateTime.Now.AddDays(expireTimes.refreshDay);
        existAccount.UpdatedAt = DateTime.Now;

        await _unitOfWork.SaveChangesAsync();

        var authResponse = new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = expireTimes.accessMinute * 60,//convert to seconds
            AccountId = existAccount.Id,
            Email = existAccount.Email,
            UserName = existAccount.Name,
            Role = existAccRole.Role.Name
        };
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_LOGIN_MSG, authResponse);
    }

    public async Task RevokeRefreshTokenAsync(Guid accountId)
    {
        var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
        if (account != null)
        {
            account.RefreshToken = null;
            account.RefreshTokenExpiryTime = null;
            account.UpdatedAt = DateTime.Now;
            await _unitOfWork.SaveChangesAsync();
        }
    }

    //=================================================================================================

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

    public async Task<IBusinessResult> GetAccountsListAsync(AccountRequest queryCondition)
    {
        return null;
        //var (result, totalCount) = await _unitOfWork.GetAccountRepository().GetAccountsListAsync
        //    (queryCondition.KeyWord,
        //    queryCondition.Gender,
        //    queryCondition.Role,
        //    queryCondition.Status);

        //if (totalCount == 0) { throw new NotFoundException("Not found any account"); }

        //// Convert to return data type
        //var mappedResult = result.Adapt<List<BM_AccountBaseInfo>>();

        //return new BM_PagingResults<BM_AccountBaseInfo>
        //{
        //    TotalCount = totalCount,
        //    DataList = mappedResult
        //};
    }

    public async Task<BusinessResult> CreateAccountAsync(AccountRequest request)
    {
        return null;
        //if (request.)
        //    return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        //// You can add additional validation here (uniqueness, required fields, etc.)
        //var account = request.Adapt<Account>();
        //account.Id = Guid.NewGuid();
        //account.CreatedAt = DateTime.UtcNow;
        //account.UpdatedAt = DateTime.UtcNow;

        //await _unitOfWork.AccountRepository.AddAsync(account);
        //var saved = await _unitOfWork.SaveChangesAsync();
        //if (saved <= 0)
        //    return new BusinessResult(Const.HTTP_STATUS_INTERNAL_SERVER_ERROR, Const.FAIL_CREATE_MSG);

        //var response = account.Adapt<AccountResponse>();
        //return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG, response);
    }

    public async Task<bool> UpdateAccountAsync(AccountRequest updateAccount)
    {
        return true;
        // Find Account of current email access this method
        //Account oldAccount = await _unitOfWork.GetAccountRepository().GetByIdAsync(updateAccount.Id)
        //    ?? throw new NotFoundException("Current Account Access Is Not Exist!");

        //oldAccount.FullName = updateAccount.FullName;
        //oldAccount.Address = updateAccount.Address;
        //oldAccount.PhoneNumber = updateAccount.PhoneNumber;
        //oldAccount.Cccd = updateAccount.Cccd;
        //oldAccount.DateOfBirth = updateAccount.DateOfBirth;
        //oldAccount.Gender = updateAccount.Gender;
        //oldAccount.LastUpdated = DateTime.Now;
        //oldAccount.Expertise = updateAccount.Expertise;
        //oldAccount.Language = updateAccount.Language;
        //oldAccount.CompanyName = updateAccount.CompanyName;

        //return await _unitOfWork.GetAccountRepository().SaveChangeAsync();
    }

    public async Task<bool> ChangeRoleAccountAsync(AccountRequest clientRequest, Guid targetAid)
    {
        return true;
        //CheckValidEmail(clientRequest.Email);

        //// Find Account of current email access this method
        //var currentAccess = await _unitOfWork.GetAccountRepository().GetOneAsync(acc =>
        //acc.Email.ToLower().Equals(clientRequest.Email.ToLower()))
        //    ?? throw new NotFoundException("Current Account Access Is Not Exist!");

        //// Confirm Owner is doing
        //if (!currentAccess.Password.Equals(HashStringSHA256(clientRequest.Password)))
        //{ throw new BadRequestException("Sai mật khẩu! Xác nhận chủ tài khoản lỗi!"); }

        ////Default target
        //Account targetChangeRole = currentAccess;

        //if (targetAid != Guid.Empty)
        //{
        //    //If specific id target is different
        //    if (currentAccess.Id != targetAid)
        //    {
        //        targetChangeRole = await _unitOfWork.GetAccountRepository().GetByIdAsync(targetAid)
        //            ?? throw new NotFoundException("The selected account to delete is not exist!");
        //    }

        //    //If non-admin target admin
        //    if (!currentAccess.Role.ToLower().Equals("admin") ||
        //        !currentAccess.Role.ToLower().Equals("nhân viên quản lý"))
        //    {
        //        if (clientRequest.Role.ToLower().Equals("admin") ||
        //            clientRequest.Role.ToLower().Equals("nhân viên quản lý"))
        //        {
        //            throw new UnauthorizedException("You don't have permission to change role of that Account!");
        //        }
        //    }

        //    targetChangeRole.Role = clientRequest.Role;
        //}
        //return await _unitOfWork.GetAccountRepository().SaveChangeAsync();
    }

    public async Task<bool> ChangePasswordAccountAsync(AuthRequest clientRequest, Guid targetAid, string newPassword)
    {
        return true;
        //CheckValidEmail(clientRequest.Email);

        //// Find Account of current email access this method
        //var currentAccess = await _unitOfWork.GetAccountRepository().GetOneAsync(acc =>
        //acc.Email.ToLower().Equals(clientRequest.Email.ToLower()))
        //    ?? throw new NotFoundException("Current Account Access Is Not Exist!");

        //// Confirm Owner is doing
        //if (!currentAccess.Password.Equals(HashStringSHA256(clientRequest.Password)))
        //{ throw new BadRequestException("Sai mật khẩu! Xác nhận chủ tài khoản lỗi!"); }

        ////Default target
        //Account targetChangePass = currentAccess;

        //if (targetAid != Guid.Empty)
        //{
        //    //If specific id target is different
        //    if (currentAccess.Id != targetAid)
        //    {
        //        targetChangePass = await _unitOfWork.GetAccountRepository().GetByIdAsync(targetAid)
        //            ?? throw new NotFoundException("The selected account to change password is not exist!");
        //    }

        //    //If non-admin target admin
        //    if (!currentAccess.Role.ToLower().Equals("admin") ||
        //        !currentAccess.Role.ToLower().Equals("nhân viên quản lý"))
        //    {
        //        if (targetChangePass.Role.ToLower().Equals("admin") ||
        //            targetChangePass.Role.ToLower().Equals("nhân viên quản lý"))
        //        {
        //            throw new UnauthorizedException("You don't have permission to change password of that Account!");
        //        }
        //    }

        //    targetChangePass.Password = HashStringSHA256(newPassword);
        //}
        //return await _unitOfWork.GetAccountRepository().SaveChangeAsync();
    }

    public async Task<bool> DeleteAccountAsync(Guid targetAid)
    {
        //CheckValidEmail(clientRequest.Email);
        return true;
    }
}