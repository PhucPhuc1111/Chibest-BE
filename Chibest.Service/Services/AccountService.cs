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
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using static Chibest.Common.Const;
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
            Email = existAccount.Email,
            UserName = existAccount.Name,
            Role = existAccRole.Role.Name,
            BranchId = existAccRole.BranchId,
            Avatar = existAccount?.AvatarUrl
        };
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_LOGIN_MSG, authResponse);
    }

    public async Task<IBusinessResult> RefreshTokenAsync(AuthTokenRequest request)
    {
        //Decode token to get accountId
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var accountId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        // Get accountId from token
        if (accountId == null || accountId.Equals(Guid.Empty.ToString()))
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Get account from accountId
        var existAccount = await _unitOfWork.AccountRepository.GetByIdAsync(Guid.Parse(accountId));
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
            Email = existAccount.Email,
            UserName = existAccount.Name,
            Role = existAccRole.Role.Name,
            BranchId = existAccRole.BranchId
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

    public async Task<IBusinessResult> GetAccountsListAsync(int pageIndex, int pageSize, string? search)
    {
        if (pageIndex <= 0) pageIndex = 1;
        if (pageSize <= 0) pageSize = 10;

        Expression<Func<Account, bool>>? predicate = null;

        if (!string.IsNullOrWhiteSpace(search))
        {
            predicate = q => q.Code.ToLower().Contains(search.ToLower()) ||
                            q.Email.ToLower().Contains(search.ToLower()) ||
                            q.Name.ToLower().Contains(search.ToLower()) ||
                            q.PhoneNumber!.Contains(search);
        }

        Func<IQueryable<Account>, IOrderedQueryable<Account>> orderBy =
            q => q.OrderByDescending(x => x.CreatedAt);

        var accounts = await _unitOfWork.AccountRepository.GetPagedAsync(
            pageIndex, pageSize, predicate, orderBy);

        var totalCount = await _unitOfWork.AccountRepository.CountAsync();

        var response = accounts.Adapt<List<AccountResponse>>();
        var pagedResult = new PagedResult<AccountResponse>
        {
            DataList = response,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, pagedResult);
    }

    public async Task<IBusinessResult> CreateAccountAsync(AccountRequest request)
    {
        // Validate input
        if (request == null || BoolUtils.IsEmptyString(request.Code, request.Email,
            request.Password, request.Name, request.Status))
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG + " Null input");

        // Check if code, email already exists
        var existingAccount = await _unitOfWork.AccountRepository
            .GetByWhere(acc => acc.Code.ToLower().Equals(request.Code!.ToLower()) ||
            acc.Email.ToLower().Equals(request.Email!.ToLower()))
            .FirstOrDefaultAsync();
        if (existingAccount != null)
            return new BusinessResult(Const.HTTP_STATUS_CONFLICT, Const.FAIL_CREATE_MSG + " Exist Account");

        // Create new account
        var account = request.Adapt<Account>();
        account.Id = Guid.NewGuid();
        account.CreatedAt = DateTime.Now;
        account.Password = StringUtils.HashStringSHA256(request.Password!);//Hash Password
        account.UpdatedAt = DateTime.Now;
        await _unitOfWork.AccountRepository.AddAsync(account);

        // If Role and branch is specific
        if (request.RoleId is not null)
        {
            // Validate Role and Branch
            var existedRole = await _unitOfWork.RoleRepository.GetByIdAsync(request.RoleId);
            var existedBranch = request.BranchId.HasValue ?
                await _unitOfWork.BranchRepository.GetByIdAsync(request.BranchId) : null;

            // If those ids are valid, create AccountRole
            if (existedRole is not null)
            {
                var accountRole = new AccountRole
                {
                    AccountId = account.Id,
                    RoleId = existedRole.Id,
                    BranchId = request.BranchId,//branch can be null so don't check
                    StartDate = DateTime.Now,
                    EndDate = null
                };
                await _unitOfWork.AccountRoleRepository.AddAsync(accountRole);

                    // If role is Supplier, ensure a SupplierDebt record exists for this supplier
                    if (string.Equals(existedRole.Name, "Supplier", StringComparison.OrdinalIgnoreCase))
                    {
                        var existingSupplierDebt = await _unitOfWork.SupplierDebtRepository
                            .GetByWhere(x => x.SupplierId == account.Id)
                            .FirstOrDefaultAsync();

                        if (existingSupplierDebt == null)
                        {
                            var supplierDebt = new SupplierDebt
                            {
                                Id = Guid.NewGuid(),
                                SupplierId = account.Id,
                                TotalDebt = 0,
                                PaidAmount = 0,
                                ReturnAmount = 0,
                            };
                            await _unitOfWork.SupplierDebtRepository.AddAsync(supplierDebt);
                        }
                    }
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG);
    }

    public async Task<IBusinessResult> UpdateAccountAsync(AccountRequest newData)
    {
        // Validation
        if (newData.Id == Guid.Empty || newData.Id is null || newData == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);
        if (BoolUtils.IsEmptyString(newData.Code, newData.Email, newData.Name, newData.Status))
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Find if exist
        var existingAccount = await _unitOfWork.AccountRepository.GetByIdAsync(newData.Id);
        if (existingAccount == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        // Check for duplicate email if email is being updated
        if (!newData.Email!.Equals(existingAccount.Email))
        {
            var duplicateEmail = await _unitOfWork.AccountRepository
                .GetByWhere(acc => acc.Email.ToLower().Equals(newData.Email.ToLower()) &&
                acc.Id != newData.Id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (duplicateEmail != null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Email already exists");
        }

        // Update properties
        var tempPassword = existingAccount.Password;
        newData.Adapt(existingAccount);

        existingAccount.Password = tempPassword; // Retain existing password
        existingAccount.UpdatedAt = DateTime.Now;

        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);
    }

    public async Task<IBusinessResult> ChangeAccountStatusAsync(Guid accountId, string status, Guid makerId)
    {
        if (accountId == Guid.Empty || string.IsNullOrEmpty(status))
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
        if (account == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        account.Status = status;
        account.UpdatedAt = DateTime.Now;
        var oldValue = JsonSerializer.Serialize(account);

        _unitOfWork.AccountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync();


        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);
    }

    public async Task<IBusinessResult> ChangeAccountPasswordAsync(Guid accountId, string newPassword)
    {
        // Validation
        if (accountId == Guid.Empty || string.IsNullOrEmpty(newPassword))
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
        if (account == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        //Secure new password
        account.Password = StringUtils.HashStringSHA256(newPassword); ;
        account.UpdatedAt = DateTime.Now;

        _unitOfWork.AccountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);
    }

    public async Task<IBusinessResult> UpdateAvatarAsync(Guid accountId, string avatarUrl)
    {
        if (accountId == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
        if (account == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        account.AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl;
        account.UpdatedAt = DateTime.Now;

        _unitOfWork.AccountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG, new { account.AvatarUrl });
    }

    public async Task<IBusinessResult> DeleteAccountAsync(Guid id)
    {
        if (id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var account = await _unitOfWork.AccountRepository.GetByIdAsync(id);
        if (account == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        _unitOfWork.AccountRepository.Delete(account);
        await _unitOfWork.SaveChangesAsync();


        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }

    public async Task<IBusinessResult> GetSupplierAccountsAsync()
    {
        var supplierRole = await _unitOfWork.RoleRepository
            .GetByWhere(r => r.Name == "Supplier")
            .FirstOrDefaultAsync();

        if (supplierRole == null)
        {
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy role Nhà Cung Cấp");
        }

        var supplierAccounts = await _unitOfWork.AccountRepository
            .GetByWhere(a => a.AccountRoles.Any(ar =>
                ar.RoleId == supplierRole.Id &&
                ar.EndDate == null))
            .Include(a => a.AccountRoles)
                .ThenInclude(ar => ar.Role)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        var response = supplierAccounts.Select(account => new AccountResponse
        {
            Id = account.Id,
            FcmToken = account.FcmToken,
            RefreshToken = account.RefreshToken,
            RefreshTokenExpiryTime = account.RefreshTokenExpiryTime,
            AvartarUrl = account.AvatarUrl,
            Code = account.Code,
            Email = account.Email,
            Name = account.Name,
            PhoneNumber = account.PhoneNumber,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            Status = account.Status
        }).ToList();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

}